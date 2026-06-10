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
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(315, 120);

        var label = new Label
        {
            AutoSize = true,
            Location = new Point(16, 18),
            Text = "Comprobar cada cuántos minutos:"
        };

        _minutes = new NumericUpDown
        {
            Location = new Point(220, 15),
            Minimum = AppSettings.MinimumIntervalMinutes,
            Maximum = AppSettings.MaximumIntervalMinutes,
            Value = currentValue,
            Width = 75
        };

        var acceptButton = new Button
        {
            DialogResult = DialogResult.OK,
            Location = new Point(139, 70),
            Text = "Aceptar",
            Width = 75
        };

        var cancelButton = new Button
        {
            DialogResult = DialogResult.Cancel,
            Location = new Point(220, 70),
            Text = "Cancelar",
            Width = 75
        };

        AcceptButton = acceptButton;
        CancelButton = cancelButton;
        Controls.AddRange([label, _minutes, acceptButton, cancelButton]);
    }
}
