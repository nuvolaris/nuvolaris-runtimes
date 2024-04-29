# nuv action invoke ##packagename##/##actionname## --param setupapisix true -r

# Esegue il comando e cattura l'output in formato JSON
$command = "nuv action invoke ##packagename##/##actionname## --param setupapisix TRUE -r"
$jsonOutput = Invoke-Expression $command | ConvertFrom-Json

# Verifica se la chiave 'openapi_spec' è presente
if ($jsonOutput.body.openapi_spec) {
    $openapiSpec = $jsonOutput.body.openapi_spec

    # Converti l'oggetto OpenAPI spec in JSON formattato
    $jsonFormatted = $openapiSpec | ConvertTo-Json -Depth 100

    # Percorso del file di output nella cartella superiore
    $outputPath = Join-Path (Get-Location) "openapi.json"

    # Salva il JSON formattato nel file, sovrascrivendo se esiste
    $jsonFormatted | Set-Content $outputPath

    Write-Host "File openapi.json salvato in: $outputPath"
} else {
    Write-Host "La chiave 'openapi_spec' non è stata trovata nell'output del comando."
}
