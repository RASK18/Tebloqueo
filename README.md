# Tebloqueo

Tebloqueo es una mini aplicación para la bandeja del sistema de Windows. Consulta periódicamente el listado de IP de `hayahora.futbol` y muestra un icono claro:

- **SI** en rojo cuando hay más de dos IP.
- **NO** en verde cuando hay cero, una o dos IP.
- **?** en gris cuando no se puede obtener o validar la respuesta.

## Uso

Descarga `Tebloqueo.exe` desde la [última release](https://github.com/RASK18/Tebloqueo/releases/latest), guárdalo en una ubicación permanente y ejecútalo. El menú del icono permite cambiar el intervalo, activar notificaciones, iniciar con Windows y salir.

La aplicación busca una actualización al arrancar. Cuando existe una versión más reciente, verifica su SHA-256, sustituye el ejecutable y se reinicia automáticamente.

## Desarrollo

Requisitos:

- Windows x64
- .NET SDK 10

Comandos principales:

```powershell
dotnet restore Tebloqueo.slnx
dotnet test Tebloqueo.slnx
dotnet run --project src/Tebloqueo/Tebloqueo.csproj
dotnet publish src/Tebloqueo/Tebloqueo.csproj -c Release -o publish
```

## Organización

- `src/Tebloqueo`: aplicación WinForms y autoactualizador.
- `tests/Tebloqueo.Tests`: pruebas automatizadas.
- `web`: placeholder de la futura web de descarga.
- `.github/workflows/app.yml`: valida ramas y pull requests; en `main`, versiona y publica el EXE.
- `.github/workflows/pages.yml`: despliega los cambios de `web` en GitHub Pages.

Las versiones publicadas usan el formato `1.0.n`. El workflow consulta las releases existentes e incrementa automáticamente `n`.
