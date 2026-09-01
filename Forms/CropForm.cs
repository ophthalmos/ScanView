using System.Drawing.Drawing2D;

namespace ScanView.Forms;

/// <summary>Zuschneide-Dialog: Auswahlrahmen mit acht Griffen über der Seitenvorschau —
/// Interaktionslogik nach dem Vorbild von Wilhelms ImageCropper (ohne Seitenverhältnis-Zwang).</summary>
internal sealed class CropForm : Form
{
    private enum DragHandle { None, TopLeft, Top, TopRight, Left, Right, BottomLeft, Bottom, BottomRight, Inside }

    private readonly PictureBox pictureBox;
    private readonly Panel scrollPanel;
    private readonly ComboBox comboZoom;
    private readonly Button btnCrop;
    private readonly Label labelSize;
    private readonly Image image;
    private readonly int handleSize;
    private Rectangle selectionRect = Rectangle.Empty; // in PictureBox-Koordinaten
    private Point mouseDownPoint;
    private Point dragStartPoint;
    private DragHandle currentDragHandle = DragHandle.None;
    private bool isDragging;
    private bool isNewSelection;

    /// <summary>Der gewählte Ausschnitt in Bildpixeln (gültig nach DialogResult.OK).</summary>
    public Rectangle SelectionInImage { get; private set; }

    public CropForm(Image image)
    {
        this.image = image;
        Text = "Zuschneiden";
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(500, 400);
        ShowIcon = false;
        ShowInTaskbar = false;
        KeyPreview = true;
        handleSize = (int)(16 * DeviceDpi / 96.0);

        Panel bottom = new() { Dock = DockStyle.Bottom, Height = 44 };
        labelSize = new Label() { AutoSize = true, Location = new Point(12, 14), Text = "Rahmen aufziehen oder Griffe verschieben" };
        btnCrop = new Button() { Text = "&Zuschneiden", DialogResult = DialogResult.OK, Size = new Size(100, 26), Enabled = false,
            Anchor = AnchorStyles.Right | AnchorStyles.Top };
        Button btnCancel = new() { Text = "Abbrechen", DialogResult = DialogResult.Cancel, Size = new Size(90, 26),
            Anchor = AnchorStyles.Right | AnchorStyles.Top };
        Label labelZoom = new() { AutoSize = true, Text = "&Zoom:", Anchor = AnchorStyles.Right | AnchorStyles.Top };
        comboZoom = new ComboBox() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 96, Anchor = AnchorStyles.Right | AnchorStyles.Top };
        comboZoom.Items.AddRange(["Einpassen", "50 %", "75 %", "100 %", "150 %", "200 %"]);
        comboZoom.SelectedIndex = 0;
        comboZoom.SelectedIndexChanged += (s, e) => ApplyZoom();
        bottom.Controls.AddRange([labelSize, labelZoom, comboZoom, btnCrop, btnCancel]);
        bottom.Resize += (s, e) =>
        {
            btnCancel.Location = new Point(bottom.Width - 102, 9);
            btnCrop.Location = new Point(bottom.Width - 210, 9);
            comboZoom.Location = new Point(bottom.Width - 322, 10);
            labelZoom.Location = new Point(bottom.Width - 366, 14);
        };
        AcceptButton = btnCrop;
        CancelButton = btnCancel;

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
        Controls.Add(bottom);

        // Dialoggröße: Bild möglichst groß, aber in den Arbeitsbereich eingepasst
        var work = Screen.FromPoint(Cursor.Position).WorkingArea;
        var scale = Math.Min(0.85 * work.Width / image.Width, 0.85 * (work.Height - bottom.Height) / image.Height);
        scale = Math.Min(scale, 1.0);
        ClientSize = new Size(Math.Max(500, (int)(image.Width * scale)), Math.Max(360, (int)(image.Height * scale) + bottom.Height));
        Shown += (s, e) => ApplyZoom();
    }

    /// <summary>Setzt die Bildgröße gemäß Zoomstufe (Index 0 = Einpassen) und erstellt die Startauswahl neu.</summary>
    private void ApplyZoom()
    {
        Size target;
        if (comboZoom.SelectedIndex <= 0)
        {
            var client = scrollPanel.ClientSize;
            var scale = Math.Min((double)client.Width / image.Width, (double)client.Height / image.Height);
            target = new Size(Math.Max(1, (int)(image.Width * scale)), Math.Max(1, (int)(image.Height * scale)));
        }
        else
        {
            var percent = int.Parse(comboZoom.Text.Split(' ')[0]);
            target = new Size(image.Width * percent / 100, image.Height * percent / 100);
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
        btnCrop.Enabled = valid;
        if (valid)
        {
            var real = TranslateToImage(selectionRect);
            SelectionInImage = real;
            labelSize.Text = $"Ausschnitt: {real.Width} × {real.Height} Pixel";
        }
        else
        {
            labelSize.Text = "Rahmen aufziehen oder Griffe verschieben";
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
