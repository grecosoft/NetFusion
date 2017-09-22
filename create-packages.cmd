msbuild "/t:Restore;Pack" .\NetFusion.sln /p:VersionPrefix=2.0.77
for /r ./src %%x in (*.2.0.77.nupkg) do copy "%%x" ..\_packages
for /r ./src %%x in (*.2.0.77.symbols.nupkg) do copy "%%x" ..\_packages