#!/bin/bash

# Generazione del file unique.ai che serve per avere tutto il codice insieme e darlo in pasto a chatgpt se necessario
nuv -task

# Pulizia dell'ambiente di build
dotnet clean
rm -rf bin
rm -rf obj

# Compilazione del progetto
dotnet build -c Release

# Preparazione della cartella di output
rm -rf out
mkdir -p out

# Copia dei file necessari nella cartella di output
cp bin/Release/net7.0/*.* out/
#cp bin/Release/net7.0/$project_name.exe out/
cp OWUtils/template.yaml out/

# Creazione dell'archivio zip
cd out
# Ottieni il nome del progetto
project_name=$(find . -maxdepth 1 -type f -executable -name "*.exe" | sed 's|./||' | sed 's|\.exe$||' | head -n 1)
# Crea l'archivio zip
zip $project_name.zip *.dll

# Esecuzione del programma per generare il file di configurazione
chmod +x ./$project_name.exe
./$project_name.exe generate

# Pulizia dei file non necessari
find . -type f ! -name '*.zip' ! -name '*.yml' ! -name '*.sh' -delete

# Deploy e invocazione dell'azione
nuv -wsk project undeploy
nuv -wsk project deploy
./invoke.sh

# Pausa di 5 secondi per visualizzare l'output
sleep 5
