namespace Tebloqueo;

internal sealed class IntervalDialog : Form
{
    private readonly NumericUpDown _minutes;

    public int IntervalMinutes => decimal.ToInt32(_minutes.Value);

    public IntervalDialog(int currentValue)
    {
        Text = "Intervalo de comprobación";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterScreen;
        AutoScaleMode = AutoScaleMode.Dpi;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(400, 170);
        Padding = new Padding(20);

        var layout = new TableLayoutPanel
        {
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            RowCount = 4
        };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));

        var label = new Label
        {
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 8),
            Text = "Comprobar cada cuántos minutos:"
        };

        _minutes = new NumericUpDown
        {
            Anchor = AnchorStyles.Left,
            Margin = new Padding(0),
            Minimum = AppSettings.MinimumIntervalMinutes,
            Maximum = AppSettings.MaximumIntervalMinutes,
            Value = currentValue,
            Width = 140
        };

        var acceptButton = new Button
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            DialogResult = DialogResult.OK,
            Margin = new Padding(8, 0, 0, 0),
            MinimumSize = new Size(96, 34),
            Padding = new Padding(8, 2, 8, 2),
            Text = "Aceptar",
        };

        var cancelButton = new Button
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            DialogResult = DialogResult.Cancel,
            Margin = new Padding(8, 0, 0, 0),
            MinimumSize = new Size(96, 34),
            Padding = new Padding(8, 2, 8, 2),
            Text = "Cancelar",
        };

        var buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            Margin = new Padding(0),
            WrapContents = false
        };
        buttonPanel.Controls.AddRange([cancelButton, acceptButton]);

        AcceptButton = acceptButton;
        CancelButton = cancelButton;

        layout.Controls.Add(label, 0, 0);
        layout.Controls.Add(_minutes, 0, 1);
        layout.Controls.Add(buttonPanel, 0, 3);
        Controls.Add(layout);
    }
}
