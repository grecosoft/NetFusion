msbuild "/t:Restore;Pack" .\NetFusion.sln /p:VersionPrefix=2.0.71
for /r ./src %%x in (*.2.0.71.nupkg) do copy "%%x" ..\_packages
for /r ./src %%x in (*.2.0.71.symbols.nupkg) do copy "%%x" ..\_packages