using System.Drawing.Drawing2D;
using ScanView.Classes;

namespace ScanView.Forms;

/// <summary>Zuschneide-Dialog: Auswahlrahmen mit acht Griffen über der Seitenvorschau —
/// Interaktionslogik nach dem Vorbild von Wilhelms ImageCropper (ohne Seitenverhältnis-Zwang).
/// Die Aktionen (Freistellen/Zuschneiden/Ausschneiden) wirken sofort auf das Vorschaubild und
/// lassen sich beliebig kombinieren; „Übernehmen" liefert das Gesamtergebnis, Esc verwirft.</summary>
internal sealed partial class CropForm : Form, IMessageFilter
{
    private enum DragHandle
    {
        None, TopLeft, Top, TopRight, Left, Right, BottomLeft, Bottom, BottomRight, Inside
    }

    private const int WM_MOUSEWHEEL = 0x020A;

    /// <summary>Strg+Mausrad über der Vorschau blättert durch die Zoomstufen (wie in der Übersicht).</summary>
    public bool PreFilterMessage(ref Message m)
    {
        if (m.Msg != WM_MOUSEWHEEL || (ModifierKeys & Keys.Control) == 0) { return false; }
        if (Form.ActiveForm != this) { return false; } // nur reagieren, wenn dieser Dialog vorn ist
        if (!scrollPanel.RectangleToScreen(scrollPanel.ClientRectangle).Contains(Cursor.Position)) { return false; }
        var delta = (short)((long)m.WParam >> 16);
        comboZoom.SelectedIndex = delta > 0
            ? Math.Min(comboZoom.Items.Count - 1, comboZoom.SelectedIndex + 1)
            : Math.Max(0, comboZoom.SelectedIndex - 1);
        return true; // nicht zusätzlich scrollen
    }

    private readonly Font applyBoldFont; // Übernehmen wird fett, sobald es Änderungen gibt
    private readonly Image image; // Original (gehört dem Aufrufer)
    private Image workingImage;   // Arbeitskopie, auf der die Aktionen sichtbar ausgeführt werden
    private string lastActionText;
    private readonly int handleSize;
    private Rectangle selectionRect = Rectangle.Empty; // in PictureBox-Koordinaten
    private Point mouseDownPoint;
    private Point dragStartPoint;
    private DragHandle currentDragHandle = DragHandle.None;
    private bool isDragging;
    private bool isNewSelection;

    /// <summary>Der aktuelle Ausschnitt in Bildpixeln (für die laufende Auswahl).</summary>
    public Rectangle SelectionInImage
    {
        get; private set;
    }

    /// <summary>Das bearbeitete Bild (gültig nach DialogResult.OK, wenn Edited true ist).</summary>
    public Image ResultImage => workingImage;

    /// <summary>True, sobald mindestens eine Aktion ausgeführt wurde.</summary>
    public bool Edited => !ReferenceEquals(workingImage, image);

    public CropForm(Image image, Rectangle storedBounds)
    {
        this.image = image;
        workingImage = image;
        InitializeComponent();
        Lng.Apply(this);
        Lng.TranslateItems(comboZoom); // nur "Einpassen" ist Text; die Prozentstufen bleiben neutral
        handleSize = (int)(16 * DeviceDpi / 96.0);
        var edge = LogicalToDeviceUnits(24); // 24-px-Symbole wie in PDFlight; der Designer-Wert gilt für 96 dpi
        toolStrip.ImageScalingSize = new Size(edge, edge);
        applyBoldFont = new Font(toolStrip.Font, FontStyle.Bold);
        try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } // Programm-Icon in der Titelzeile
        catch (Exception ex) when (ex is ArgumentException or IOException) { }
        pictureBox.Image = image;

        if (ToolbarIcons.FontAvailable)
        {
            btnZoomOut.Image = ToolbarIcons.Get(ToolbarIcons.ZoomOut, toolStrip.ImageScalingSize);
            btnZoomIn.Image = ToolbarIcons.Get(ToolbarIcons.ZoomIn, toolStrip.ImageScalingSize);
            btnIsolate.Image = ToolbarIcons.Get(ToolbarIcons.SinglePage, toolStrip.ImageScalingSize);
            btnCropAction.Image = ToolbarIcons.Get(ToolbarIcons.Crop, toolStrip.ImageScalingSize);
            btnRemove.Image = ToolbarIcons.Get(ToolbarIcons.Cut, toolStrip.ImageScalingSize);
            btnApply.Image = ToolbarIcons.Get(ToolbarIcons.Accept, toolStrip.ImageScalingSize);
            btnCancel.Image = ToolbarIcons.Get(ToolbarIcons.Cancel, toolStrip.ImageScalingSize);
        }
        else
        {
            btnZoomOut.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnZoomIn.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnZoomOut.Text = "−";
            btnZoomIn.Text = "+";
        }
        comboZoom.SelectedIndex = 0; // Einpassen
        Application.AddMessageFilter(this); // Strg+Mausrad-Zoom (s. PreFilterMessage)

        if (storedBounds.Width >= MinimumSize.Width && storedBounds.Height >= MinimumSize.Height
            && Screen.AllScreens.Any(screen => screen.WorkingArea.IntersectsWith(storedBounds))) // gemerkte Größe/Position
        {
            StartPosition = FormStartPosition.Manual;
            Bounds = storedBounds;
        }
        else // erster Aufruf: Bild möglichst groß, aber in den Arbeitsbereich eingepasst
        {
            var work = Screen.FromPoint(Cursor.Position).WorkingArea;
            var chrome = toolStrip.Height + statusStrip.Height;
            var scale = Math.Min(0.85 * work.Width / image.Width, 0.85 * (work.Height - chrome) / image.Height);
            scale = Math.Min(scale, 1.0);
            ClientSize = new Size(Math.Max(560, (int)(image.Width * scale)), Math.Max(360, (int)(image.Height * scale) + chrome));
        }
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        switch (keyData)
        {
            case Keys.Escape: DialogResult = DialogResult.Cancel; return true; // Esc verwirft und schließt
            case Keys.Enter when btnApply.Enabled: DialogResult = DialogResult.OK; return true;
        }
        return base.ProcessCmdKey(ref msg, keyData);
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        base.OnFormClosed(e);
        Application.RemoveMessageFilter(this);
        if (DialogResult != DialogResult.OK && Edited) // verworfen — die Arbeitskopie aufräumen
        {
            pictureBox.Image = image;
            workingImage.Dispose();
            workingImage = image;
        }
    }

    // ------------------------------------------------------------------ Ereignishandler des Designers

    private void CropForm_Shown(object sender, EventArgs e)
    {
        ApplyZoom();
    }

    private void ComboZoom_SelectedIndexChanged(object sender, EventArgs e)
    {
        ApplyZoom();
    }

    private void BtnZoomOut_Click(object sender, EventArgs e)
    {
        comboZoom.SelectedIndex = Math.Max(0, comboZoom.SelectedIndex - 1);
    }

    private void BtnZoomIn_Click(object sender, EventArgs e)
    {
        comboZoom.SelectedIndex = Math.Min(comboZoom.Items.Count - 1, comboZoom.SelectedIndex + 1);
    }

    private void BtnIsolate_Click(object sender, EventArgs e)
    {
        IsolateSelection();
    }

    private void BtnCropAction_Click(object sender, EventArgs e)
    {
        CropToSelection();
    }

    private void BtnRemove_Click(object sender, EventArgs e)
    {
        RemoveSelection();
    }

    private void BtnApply_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.OK;
    }

    private void BtnCancel_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
    }

    private void ScrollPanel_Resize(object sender, EventArgs e)
    {
        if (comboZoom.SelectedIndex == 0) { ApplyZoom(); }
        else { CenterPictureBox(); }
    }

    private void PictureBox_Resize(object sender, EventArgs e)
    {
        selectionRect = Rectangle.Empty; // die Umrechnung stimmt sonst nicht mehr
        UpdateUiState();
    }

    // ------------------------------------------------------------------ Aktionen (sofort sichtbar, wiederholbar)

    /// <summary>Freistellen: außerhalb der Auswahl weiß, Bildgröße bleibt.</summary>
    private void IsolateSelection()
    {
        var rect = SelectionInImage;
        Bitmap result = new(workingImage.Width, workingImage.Height);
        result.SetResolution(workingImage.HorizontalResolution, workingImage.VerticalResolution);
        using (var g = Graphics.FromImage(result))
        {
            g.Clear(Color.White);
            g.DrawImage(workingImage, rect, rect, GraphicsUnit.Pixel); // Auswahl bleibt an ihrer Position
        }
        ReplaceWorkingImage(result, Lng.T("Freigestellt: außerhalb der Auswahl weiß"));
    }

    /// <summary>Zuschneiden: das Bild wird auf die Auswahl verkleinert.</summary>
    private void CropToSelection()
    {
        var rect = SelectionInImage;
        Bitmap result = new(rect.Width, rect.Height);
        result.SetResolution(workingImage.HorizontalResolution, workingImage.VerticalResolution);
        using (var g = Graphics.FromImage(result))
        {
            g.DrawImage(workingImage, new Rectangle(0, 0, rect.Width, rect.Height), rect, GraphicsUnit.Pixel);
        }
        ReplaceWorkingImage(result, string.Format(Lng.T("Zugeschnitten auf {0} × {1} Pixel"), rect.Width, rect.Height));
    }

    /// <summary>Ausschneiden: die Auswahl wird weiß entfernt, Bildgröße bleibt.</summary>
    private void RemoveSelection()
    {
        var rect = SelectionInImage;
        Bitmap result = new(workingImage.Width, workingImage.Height);
        result.SetResolution(workingImage.HorizontalResolution, workingImage.VerticalResolution);
        using (var g = Graphics.FromImage(result))
        {
            g.DrawImage(workingImage, new Rectangle(0, 0, result.Width, result.Height), new Rectangle(0, 0, result.Width, result.Height), GraphicsUnit.Pixel);
            g.FillRectangle(Brushes.White, rect);
        }
        ReplaceWorkingImage(result, Lng.T("Ausgeschnitten: die Auswahl wurde weiß entfernt"));
    }

    /// <summary>Tauscht die Arbeitskopie aus und zeigt das Ergebnis sofort in der Vorschau.</summary>
    private void ReplaceWorkingImage(Bitmap result, string actionText)
    {
        var previous = workingImage;
        workingImage = result;
        pictureBox.Image = workingImage;
        if (!ReferenceEquals(previous, image)) { previous.Dispose(); }
        lastActionText = actionText + "   ·   " + Lng.T("Übernehmen speichert, Esc verwirft alles");
        ApplyZoom(createDefaultSelection: false); // Ergebnis unverdeckt zeigen — die nächste Auswahl zieht man neu auf
    }

    /// <summary>Setzt die Bildgröße gemäß Zoomstufe (Index 0 = Einpassen) und erstellt die Startauswahl
    /// neu — nach einer Aktion bleibt die Auswahl ausgeblendet, damit das Ergebnis unverdeckt sichtbar ist.</summary>
    private void ApplyZoom(bool createDefaultSelection = true)
    {
        Size target;
        if (comboZoom.SelectedIndex <= 0)
        {
            var client = scrollPanel.ClientSize;
            var scale = Math.Min((double)client.Width / workingImage.Width, (double)client.Height / workingImage.Height);
            target = new Size(Math.Max(1, (int)(workingImage.Width * scale)), Math.Max(1, (int)(workingImage.Height * scale)));
        }
        else
        {
            var percent = int.Parse(comboZoom.Text.Split(' ')[0]);
            target = new Size(workingImage.Width * percent / 100, workingImage.Height * percent / 100);
        }
        if (pictureBox.Size != target) { pictureBox.Size = target; } // der Resize-Handler leert die Auswahl
        scrollPanel.AutoScrollPosition = Point.Empty;
        CenterPictureBox();
        if (createDefaultSelection) { CreateDefaultSelection(); }
        else { selectionRect = Rectangle.Empty; UpdateUiState(); }
    }

    /// <summary>Zentriert das Bild im Scrollbereich, solange es kleiner als der Bereich ist.</summary>
    private void CenterPictureBox()
    {
        var x = Math.Max(0, (scrollPanel.ClientSize.Width - pictureBox.Width) / 2);
        var y = Math.Max(0, (scrollPanel.ClientSize.Height - pictureBox.Height) / 2);
        pictureBox.Location = new Point(scrollPanel.AutoScrollPosition.X + x, scrollPanel.AutoScrollPosition.Y + y);
    }

    /// <summary>Startauswahl: 90 % der Bildfläche, zentriert.</summary>
    private void CreateDefaultSelection()
    {
        var valid = GetImageRectangle();
        if (valid.Width <= 0 || valid.Height <= 0) { return; }
        var w = (int)(valid.Width * 0.9);
        var h = (int)(valid.Height * 0.9);
        selectionRect = new Rectangle(valid.X + (valid.Width - w) / 2, valid.Y + (valid.Height - h) / 2, w, h);
        UpdateUiState();
    }

    /// <summary>Der Bereich der PictureBox, den das gezoomte Bild tatsächlich einnimmt.</summary>
    private Rectangle GetImageRectangle()
    {
        var img = pictureBox.Image.Size;
        var ctrl = pictureBox.ClientSize;
        var ratioImg = (float)img.Width / img.Height;
        var ratioCtrl = (float)ctrl.Width / ctrl.Height;
        int x = 0, y = 0, w = ctrl.Width, h = ctrl.Height;
        if (ratioImg > ratioCtrl) { h = (int)(w / ratioImg); y = (ctrl.Height - h) / 2; }
        else { w = (int)(h * ratioImg); x = (ctrl.Width - w) / 2; }
        return new Rectangle(x, y, w, h);
    }

    /// <summary>Rechnet die Auswahl aus PictureBox- in Bildpixel-Koordinaten um (Zoom-Modus).</summary>
    private Rectangle TranslateToImage(Rectangle selection)
    {
        var img = pictureBox.Image;
        var valid = GetImageRectangle();
        var scale = (float)img.Width / valid.Width;
        var x = (int)Math.Max(0, (selection.X - valid.X) * scale);
        var y = (int)Math.Max(0, (selection.Y - valid.Y) * scale);
        var w = (int)Math.Round(selection.Width * scale);
        var h = (int)Math.Round(selection.Height * scale);
        return new Rectangle(x, y, Math.Min(w, img.Width - x), Math.Min(h, img.Height - y));
    }

    private void PictureBox_MouseDown(object sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left) { return; }
        var valid = GetImageRectangle();
        mouseDownPoint = new Point(Math.Clamp(e.X, valid.Left, valid.Right), Math.Clamp(e.Y, valid.Top, valid.Bottom));
        currentDragHandle = HitTest(e.Location);
        if (currentDragHandle != DragHandle.None)
        {
            isDragging = true;
            dragStartPoint = e.Location;
            isNewSelection = false;
        }
        else
        {
            selectionRect = Rectangle.Empty;
            isNewSelection = true;
            isDragging = false;
            UpdateUiState();
        }
    }

    private void PictureBox_MouseMove(object sender, MouseEventArgs e)
    {
        var valid = GetImageRectangle();
        if (!isDragging && isNewSelection && e.Button == MouseButtons.Left
            && (Math.Abs(e.X - mouseDownPoint.X) > SystemInformation.DragSize.Width
                || Math.Abs(e.Y - mouseDownPoint.Y) > SystemInformation.DragSize.Height))
        {
            isDragging = true;
            dragStartPoint = mouseDownPoint;
        }
        if (!isDragging) { SetCursor(HitTest(e.Location)); return; }

        var x = Math.Clamp(e.X, valid.Left, valid.Right);
        var y = Math.Clamp(e.Y, valid.Top, valid.Bottom);
        Rectangle newRect;
        if (isNewSelection)
        {
            newRect = new Rectangle(Math.Min(mouseDownPoint.X, x), Math.Min(mouseDownPoint.Y, y),
                Math.Abs(mouseDownPoint.X - x), Math.Abs(mouseDownPoint.Y - y));
        }
        else
        {
            newRect = selectionRect;
            switch (currentDragHandle)
            {
                case DragHandle.TopLeft: newRect = Rectangle.FromLTRB(x, y, selectionRect.Right, selectionRect.Bottom); break;
                case DragHandle.TopRight: newRect = Rectangle.FromLTRB(selectionRect.X, y, x, selectionRect.Bottom); break;
                case DragHandle.BottomLeft: newRect = Rectangle.FromLTRB(x, selectionRect.Y, selectionRect.Right, y); break;
                case DragHandle.BottomRight: newRect = Rectangle.FromLTRB(selectionRect.X, selectionRect.Y, x, y); break;
                case DragHandle.Top: newRect = Rectangle.FromLTRB(selectionRect.X, y, selectionRect.Right, selectionRect.Bottom); break;
                case DragHandle.Bottom: newRect = Rectangle.FromLTRB(selectionRect.X, selectionRect.Y, selectionRect.Right, y); break;
                case DragHandle.Left: newRect = Rectangle.FromLTRB(x, selectionRect.Y, selectionRect.Right, selectionRect.Bottom); break;
                case DragHandle.Right: newRect = Rectangle.FromLTRB(selectionRect.X, selectionRect.Y, x, selectionRect.Bottom); break;
                case DragHandle.Inside:
                    newRect.Offset(e.X - dragStartPoint.X, e.Y - dragStartPoint.Y);
                    if (newRect.Left < valid.Left) { newRect.X = valid.Left; }
                    if (newRect.Top < valid.Top) { newRect.Y = valid.Top; }
                    if (newRect.Right > valid.Right) { newRect.X = valid.Right - newRect.Width; }
                    if (newRect.Bottom > valid.Bottom) { newRect.Y = valid.Bottom - newRect.Height; }
                    dragStartPoint = e.Location;
                    break;
            }
        }
        newRect = Normalize(newRect);
        newRect.Intersect(valid);
        if (newRect != selectionRect)
        {
            selectionRect = newRect;
            UpdateUiState();
        }
    }

    private void PictureBox_MouseUp(object sender, MouseEventArgs e)
    {
        isDragging = false;
        isNewSelection = false;
        currentDragHandle = DragHandle.None;
        pictureBox.Cursor = Cursors.Default;
        UpdateUiState();
    }

    private void UpdateUiState()
    {
        var valid = selectionRect.Width > 4 && selectionRect.Height > 4;
        btnIsolate.Enabled = valid;
        btnCropAction.Enabled = valid;
        btnRemove.Enabled = valid;
        btnApply.Enabled = Edited;
        btnApply.Font = Edited ? applyBoldFont : null; // fett, sobald es etwas zu übernehmen gibt (null = ToolStrip-Schrift)
        if (valid)
        {
            var real = TranslateToImage(selectionRect);
            SelectionInImage = real;
            statusLabel.Text = string.Format(Lng.T("Auswahl: {0} × {1} Pixel"), real.Width, real.Height)
                + (lastActionText == null ? string.Empty : "   ·   " + lastActionText);
        }
        else
        {
            statusLabel.Text = lastActionText ?? Lng.T("Rahmen aufziehen oder Griffe verschieben — Esc schließt ohne Änderung");
        }
        pictureBox.Invalidate();
    }

    /// <summary>Abdunkelung außerhalb der Auswahl, weißer Rahmen mit Strichlinie, acht Griffe.</summary>
    private void PictureBox_Paint(object sender, PaintEventArgs e)
    {
        if (selectionRect.IsEmpty) { return; }
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.None;
        var w = pictureBox.ClientSize.Width;
        var h = pictureBox.ClientSize.Height;
        using SolidBrush shadowBrush = new(Color.FromArgb(120, Color.Black));
        g.FillRectangle(shadowBrush, 0, 0, w, selectionRect.Y);
        g.FillRectangle(shadowBrush, 0, selectionRect.Bottom, w, h - selectionRect.Bottom);
        g.FillRectangle(shadowBrush, 0, selectionRect.Y, selectionRect.X, selectionRect.Height);
        g.FillRectangle(shadowBrush, selectionRect.Right, selectionRect.Y, w - selectionRect.Right, selectionRect.Height);
        Rectangle drawRect = new(selectionRect.X, selectionRect.Y, selectionRect.Width - 1, selectionRect.Height - 1);
        using Pen whitePen = new(Color.White, 1);
        g.DrawRectangle(whitePen, drawRect);
        using Pen dashPen = new(Color.Black, 1) { DashStyle = DashStyle.Dash };
        g.DrawRectangle(dashPen, drawRect);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        foreach (var handle in Enum.GetValues<DragHandle>())
        {
            if (handle is DragHandle.None or DragHandle.Inside) { continue; }
            DrawHandle(g, GetHandleRect(handle));
        }
    }

    private void DrawHandle(Graphics g, Rectangle r)
    {
        var shadow = r;
        shadow.Offset(Math.Max(1, handleSize / 8), Math.Max(1, handleSize / 8));
        using SolidBrush shadowBrush = new(Color.FromArgb(50, Color.Black));
        g.FillRectangle(shadowBrush, shadow);
        g.FillRectangle(Brushes.White, r);
        using Pen pen = new(Color.DarkSlateGray, handleSize > 15 ? 2 : 1);
        g.DrawRectangle(pen, r.X, r.Y, r.Width, r.Height);
    }

    private DragHandle HitTest(Point mouseLoc)
    {
        if (selectionRect.IsEmpty) { return DragHandle.None; }
        var tolerance = handleSize / 4;
        foreach (var handle in Enum.GetValues<DragHandle>())
        {
            if (handle is DragHandle.None or DragHandle.Inside) { continue; }
            var r = GetHandleRect(handle);
            r.Inflate(tolerance, tolerance);
            if (r.Contains(mouseLoc)) { return handle; }
        }
        return selectionRect.Contains(mouseLoc) ? DragHandle.Inside : DragHandle.None;
    }

    private Rectangle GetHandleRect(DragHandle handle)
    {
        var r = selectionRect;
        var half = handleSize / 2;
        return handle switch
        {
            DragHandle.TopLeft => new Rectangle(r.X - half, r.Y - half, handleSize, handleSize),
            DragHandle.TopRight => new Rectangle(r.Right - half, r.Y - half, handleSize, handleSize),
            DragHandle.BottomLeft => new Rectangle(r.X - half, r.Bottom - half, handleSize, handleSize),
            DragHandle.BottomRight => new Rectangle(r.Right - half, r.Bottom - half, handleSize, handleSize),
            DragHandle.Top => new Rectangle(r.X + r.Width / 2 - half, r.Y - half, handleSize, handleSize),
            DragHandle.Bottom => new Rectangle(r.X + r.Width / 2 - half, r.Bottom - half, handleSize, handleSize),
            DragHandle.Left => new Rectangle(r.X - half, r.Y + r.Height / 2 - half, handleSize, handleSize),
            DragHandle.Right => new Rectangle(r.Right - half, r.Y + r.Height / 2 - half, handleSize, handleSize),
            _ => Rectangle.Empty,
        };
    }

    private void SetCursor(DragHandle handle)
    {
        pictureBox.Cursor = handle switch
        {
            DragHandle.TopLeft or DragHandle.BottomRight => Cursors.SizeNWSE,
            DragHandle.TopRight or DragHandle.BottomLeft => Cursors.SizeNESW,
            DragHandle.Top or DragHandle.Bottom => Cursors.SizeNS,
            DragHandle.Left or DragHandle.Right => Cursors.SizeWE,
            DragHandle.Inside => Cursors.SizeAll,
            _ => Cursors.Default,
        };
    }

    private static Rectangle Normalize(Rectangle r) => new(
        r.Width < 0 ? r.X + r.Width : r.X,
        r.Height < 0 ? r.Y + r.Height : r.Y,
        Math.Abs(r.Width), Math.Abs(r.Height));
}
