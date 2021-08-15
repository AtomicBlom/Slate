$texturePackerPath = "C:\Users\codew\Downloads\MyraTexturePacker.0.9.2\MyraTexturePacker.exe"

Push-Location $PSScriptRoot
& $texturePackerPath ".\Assets\UI" ".\FrontEnd\Slate.Client\Content\UI\UI.png"
Pop-Location