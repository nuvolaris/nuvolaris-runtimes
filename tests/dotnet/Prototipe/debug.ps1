# Apri Visual Studio Code con il progetto
Start-Process "code" -n .

# Aspetta un po' per permettere a VS Code di avviarsi
Start-Sleep -Seconds 10

# Simula la pressione dei tasti (es. F5)
Add-Type -AssemblyName System.Windows.Forms
[System.Windows.Forms.SendKeys]::SendWait("{F5}")