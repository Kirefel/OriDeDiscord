msbuild /t:Build /restore /p:Configuration=Release /p:OutDir=..\build

rm -R .\release -ErrorAction SilentlyContinue

mkdir .\release\

cp .\build\OriDeDiscord.dll .\release\
cp .\build\discord_game_sdk.dll .\release\
cp .\mod.json .\release\

rm .\OriDeDiscord.zip -ErrorAction SilentlyContinue
powershell Compress-Archive .\release\* .\OriDeDiscord.zip
