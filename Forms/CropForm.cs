using System.Drawing.Drawing2D;
using ScanView.Classes;

namespace ScanView.Forms;

/// <summary>Zuschneide-Dialog: Auswahlrahmen mit acht Griffen über der Seitenvorschau —
/// Interaktionslogik nach dem Vorbild von Wilhelms ImageCropper (ohne Seitenverhältnis-Zwang).
/// Die Aktionen (Freistellen/Zuschneiden/Ausschneiden) wirken sofort auf das Vorschaubild und
/// lassen sich beliebig kombinieren; „Übernehmen" liefert das Gesamtergebnis, Esc verwirft.</summary>
internal sealed class CropForm : Form
{
    private enum DragHandle { None, TopLeft, Top, TopRight, Left, Right, BottomLeft, Bottom, BottomRight, Inside }

    private readonly PictureBox pictureBox;
    private readonly Panel scrollPanel;
    private readonly ToolStripComboBox comboZoom;
    private readonly ToolStripButton btnIsolate;
    private readonly ToolStripButton btnCropAction;
    private readonly ToolStripButton btnRemove;
    private readonly Button btnApply;
    private readonly ToolStripStatusLabel statusLabel;
    private readonly ToolTip toolTip = new();
    private readonly Image image; // Original (gehört dem Aufrufer)
    private Image workingImage;   // Arbeitskopie, auf der die Aktionen sichtbar ausgeführt werden
    private string lastActionText;
    private readonly int handleSize;
    private static readonly Color AccentColor = Color.FromArgb(0x2B, 0x77, 0xC0); // Übernehmen hervorheben
    private Rectangle selectionRect = Rectangle.Empty; // in PictureBox-Koordinaten
    private Point mouseDownPoint;
    private Point dragStartPoint;
    private DragHandle currentDragHandle = DragHandle.None;
    private bool isDragging;
    private bool isNewSelection;

    /// <summary>Der aktuelle Ausschnitt in Bildpixeln (für die laufende Auswahl).</summary>
    public Rectangle SelectionInImage { get; private set; }

    /// <summary>Das bearbeitete Bild (gültig nach DialogResult.OK, wenn Edited true ist).</summary>
    public Image ResultImage => workingImage;

    /// <summary>True, sobald mindestens eine Aktion ausgeführt wurde.</summary>
    public bool Edited => !ReferenceEquals(workingImage, image);

    public CropForm(Image image, Rectangle storedBounds)
    {
        this.image = image;
        workingImage = image;
        Text = "Zuschneiden";
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(560, 400);
        ShowIcon = false;
        ShowInTaskbar = false;
        handleSize = (int)(16 * DeviceDpi / 96.0);

        ToolStrip toolStrip = new() { GripStyle = ToolStripGripStyle.Hidden };
        var edge = LogicalToDeviceUnits(24); // 24-px-Symbole wie in PDFlight
        toolStrip.ImageScalingSize = new Size(edge, edge);
        toolStrip.Font = new Font(Font.FontFamily, 10f);
        ToolStripLabel labelZoom = new("&Zoom:");
        comboZoom = new ToolStripComboBox() { DropDownStyle = ComboBoxStyle.DropDownList, AutoSize = false, Width = 110 };
        comboZoom.Items.AddRange(["Einpassen", "50 %", "75 %", "100 %", "150 %", "200 %"]);
        comboZoom.SelectedIndex = 0;
        comboZoom.SelectedIndexChanged += (s, e) => ApplyZoom();
        ToolStripButton btnZoomOut = new() { DisplayStyle = ToolStripItemDisplayStyle.Image, ToolTipText = "Verkleinern" };
        ToolStripButton btnZoomIn = new() { DisplayStyle = ToolStripItemDisplayStyle.Image, ToolTipText = "Vergrößern" };
        btnZoomOut.Click += (s, e) => comboZoom.SelectedIndex = Math.Max(0, comboZoom.SelectedIndex - 1);
        btnZoomIn.Click += (s, e) => comboZoom.SelectedIndex = Math.Min(comboZoom.Items.Count - 1, comboZoom.SelectedIndex + 1);

        // Die drei Aktionen als echte Buttons: sie wirken sofort auf das Vorschaubild
        ToolStripButton MakeAction(string text, string tip, EventHandler onClick)
        {
            ToolStripButton button = new(text) { DisplayStyle = ToolStripItemDisplayStyle.ImageAndText, Enabled = false, ToolTipText = tip };
            button.Click += onClick;
            return button;
        }
        btnIsolate = MakeAction("&Freistellen", "Außerhalb der Auswahl wird weiß — die Bildgröße (z.B. A4) bleibt erhalten", (s, e) => IsolateSelection());
        btnCropAction = MakeAction("&Zuschneiden", "Das Bild wird auf die Auswahl verkleinert", (s, e) => CropToSelection());
        btnRemove = MakeAction("&Ausschneiden", "Die Auswahl wird aus dem Bild entfernt (weiß) — die Bildgröße bleibt erhalten", (s, e) => RemoveSelection());

        btnApply = new Button()
        {
            Text = "&Übernehmen",
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.Gainsboro, // bis eine Aktion ausgeführt wurde
            ForeColor = Color.White,
            Font = new Font(Font.FontFamily, 10f, FontStyle.Bold),
            Size = new Size(130, 30),
            Enabled = false,
        };
        btnApply.FlatAppearance.BorderSize = 0;
        btnApply.Click += (s, e) => DialogResult = DialogResult.OK;
        toolTip.SetToolTip(btnApply, "Ergebnis in die Seite übernehmen (Enter)");
        ToolStripControlHost applyHost = new(btnApply) { Alignment = ToolStripItemAlignment.Right, Margin = new Padding(0, 2, 8, 2) };

        Button btnCancel = new() { Text = "Abbrechen", Font = toolStrip.Font, Size = new Size(100, 30) };
        btnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;
        toolTip.SetToolTip(btnCancel, "Alle Aktionen verwerfen und schließen (Esc)");
        ToolStripControlHost cancelHost = new(btnCancel) { Alignment = ToolStripItemAlignment.Right, Margin = new Padding(0, 2, 6, 2) };

        if (ToolbarIcons.FontAvailable)
        {
            btnZoomOut.Image = ToolbarIcons.Get(ToolbarIcons.ZoomOut, toolStrip.ImageScalingSize);
            btnZoomIn.Image = ToolbarIcons.Get(ToolbarIcons.ZoomIn, toolStrip.ImageScalingSize);
            btnIsolate.Image = ToolbarIcons.Get(ToolbarIcons.SinglePage, toolStrip.ImageScalingSize);
            btnCropAction.Image = ToolbarIcons.Get(ToolbarIcons.Crop, toolStrip.ImageScalingSize);
            btnRemove.Image = ToolbarIcons.Get(ToolbarIcons.Cut, toolStrip.ImageScalingSize);
        }
        else
        {
            btnZoomOut.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnZoomIn.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnZoomOut.Text = "−";
            btnZoomIn.Text = "+";
        }
        toolStrip.Items.AddRange(new ToolStripItem[] { labelZoom, comboZoom, btnZoomOut, btnZoomIn,
            new ToolStripSeparator(), btnIsolate, btnCropAction, btnRemove, applyHost, cancelHost }); // rechts: Übernehmen ganz außen, Abbrechen links davon

        StatusStrip statusStrip = new();
        statusLabel = new ToolStripStatusLabel("Rahmen aufziehen oder Griffe verschieben — Esc schließt ohne Änderung");
        statusStrip.Items.Add(statusLabel);

        pictureBox = new PictureBox()
        {
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.FromArgb(48, 48, 48),
            Image = image,
        };
        pictureBox.MouseDown += PictureBox_MouseDown;
        pictureBox.MouseMove += PictureBox_MouseMove;
        pictureBox.MouseUp += PictureBox_MouseUp;
        pictureBox.Paint += PictureBox_Paint;
        pictureBox.Resize += (s, e) => { selectionRect = Rectangle.Empty; UpdateUiState(); }; // die Umrechnung stimmt sonst nicht mehr

        scrollPanel = new Panel() { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.FromArgb(48, 48, 48) };
        scrollPanel.Controls.Add(pictureBox);
        scrollPanel.Resize += (s, e) => { if (comboZoom.SelectedIndex == 0) { ApplyZoom(); } else { CenterPictureBox(); } };

        Controls.Add(scrollPanel);
        Controls.Add(statusStrip);
        Controls.Add(toolStrip);

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
        Shown += (s, e) => ApplyZoom();
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        switch (keyData)
        {
            case Keys.Escape: DialogResult = DialogResult.Cancel; return true; // kein Abbrechen-Button — Esc schließt
            case Keys.Enter when btnApply.Enabled: DialogResult = DialogResult.OK; return true;
        }
        return base.ProcessCmdKey(ref msg, keyData);
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        base.OnFormClosed(e);
        if (DialogResult != DialogResult.OK && Edited) // verworfen — die Arbeitskopie aufräumen
        {
            pictureBox.Image = image;
            workingImage.Dispose();
            workingImage = image;
        }
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
        ReplaceWorkingImage(result, "Freigestellt: außerhalb der Auswahl weiß");
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
        ReplaceWorkingImage(result, $"Zugeschnitten auf {rect.Width} × {rect.Height} Pixel");
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
        ReplaceWorkingImage(result, "Ausgeschnitten: die Auswahl wurde weiß entfernt");
    }

    /// <summary>Tauscht die Arbeitskopie aus und zeigt das Ergebnis sofort in der Vorschau.</summary>
    private void ReplaceWorkingImage(Bitmap result, string actionText)
    {
        var previous = workingImage;
        workingImage = result;
        pictureBox.Image = workingImage;
        if (!ReferenceEquals(previous, image)) { previous.Dispose(); }
        lastActionText = actionText + "   ·   Übernehmen speichert, Esc verwirft alles";
        ApplyZoom(); // Bildgröße kann sich geändert haben; erstellt auch die neue Standardauswahl
    }

    /// <summary>Setzt die Bildgröße gemäß Zoomstufe (Index 0 = Einpassen) und erstellt die Startauswahl neu.</summary>
    private void ApplyZoom()
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
        CreateDefaultSelection();
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
        btnApply.BackColor = Edited ? AccentColor : Color.Gainsboro; // hervorheben, sobald es etwas zu übernehmen gibt
        if (valid)
        {
            var real = TranslateToImage(selectionRect);
            SelectionInImage = real;
            statusLabel.Text = $"Auswahl: {real.Width} × {real.Height} Pixel" + (lastActionText == null ? string.Empty : "   ·   " + lastActionText);
        }
        else
        {
            statusLabel.Text = lastActionText ?? "Rahmen aufziehen oder Griffe verschieben — Esc schließt ohne Änderung";
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
