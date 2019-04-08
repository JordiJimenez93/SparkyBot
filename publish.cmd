nuget restore
msbuild CoreBot.sln -p:DeployOnBuild=true -p:PublishProfile=sparkybot-Web-Deploy.pubxml -p:Password=7PNoJMEltseiisaEpkbmyvZHRclAidjT9MBbGtzb6otfDhvcgc8Cr517RGni

