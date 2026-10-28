### Description
Mirror is an **extension** of [Reflection](https://github.com/JsonAsAsset/Reflection) that fetches almost every referenced asset for you, hands-free.

<div> 
   
   [![Discord](https://img.shields.io/badge/Join%20Discord-Collector?color=black&logo=discord&logoColor=white&style=for-the-badge)](https://discord.gg/xXEw4jc2UT)
   [![Buy Me a Coffee](https://img.shields.io/badge/Buy%20Me%20a%20Coffee-Ko--fi?color=black&logo=ko-fi&logoColor=white&style=for-the-badge)](https://ko-fi.com/tectory)
   [![Releases](https://img.shields.io/github/downloads/JsonAsAsset/Mirror/total?style=for-the-badge&color=black&label=Downloads)](https://github.com/JsonAsAsset/Mirror/releases)
</div>

<a name="installation"></a>
1. **Visit the Releases Page:**  
   Go to the [Releases page](/../../releases), and download the latest release.
2. **Setup the Application:**  
   Follow the setup process.
3. **Start a new Profile:**  
   Go to the Profiles tab (bottom left) and load a profile, or create one.
4. **Profit.**  

<a name="commands"></a>
## Git Commands
#### Clone
```
git clone https://github.com/JsonAsAsset/Mirror --recursive
```
#### Build
```
dotnet publish Source/Mirror -c Release --no-self-contained -r win-x64 -o "./Release" -p:PublishSingleFile=true -p:DebugType=None -p:DebugSymbols=false -p:IncludeNativeLibrariesForSelfExtract=true
```
#### Update Libraries
```
Dependencies\update_libraries.bat
```
#### Update Lisence After Changes
```
python .github/Licensing/main.py
```

