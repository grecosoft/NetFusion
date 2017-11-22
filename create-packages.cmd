msbuild "/t:Restore;Pack" .\NetFusion.sln /p:VersionPrefix=2.0.9
for /r ./src %%x in (*.2.0.9.nupkg) do copy "%%x" ..\_packages
for /r ./src %%x in (*.2.0.9.symbols.nupkg) do copy "%%x" ..\_packages