### setup
- install .NET SDK, not just .NET!
- install bepinex templates w `dotnet new install BepInEx.Templates::2.0.0-be.4 --nuget-source https://nuget.bepinex.dev/v3/index.json`
- begin from empty dir, following https://docs.bepinex.dev/articles/dev_guide/plugin_tutorial/2_plugin_start.html
- create template w command format `dotnet new bepinex5plugin -n <projName> -T <TFM> -U <Unity>` as `dotnet new bepinex5plugin -n sudden_death -T net46 -U 2021.3.45`
    - TFM = target framework
    - straftat `mscorlib.dll` is > v4.0, so TFM is `net46`
    - unity ver from bepinex log is `2021.3.45` i think (theres another .# after the 45...)

### hello world
- set main function attributes and proj name in csproj
- `dotnet build`
- move file to `bepinex/plugins` folder in mod manager

### making the mod!!!!
- going to need harmony to actually hook into the game, but first i decide to start looking around ILSpy, find `ScoreManager.ResetRound()` which looks promising as a hook.
- *harmony is only 1/2 of the main options, the other option is hookgenpatcher
- apparently i never added libs? create lib folder and drag in dlls for game, unity, bepinex, hookgenpatcher, and add to csproj
- so hookgenpatcher is for running at the start or end of registered events, IL/reflection/harmony is for inserting logic into the middle of events

- realize i'm getting nowhere with basic trial and error of events and i still have no idea how MMHook works. find example mod at https://github.com/kestrel-straftat/the-other-mods/blob/master/AboubiAcrobatics that uses harmony