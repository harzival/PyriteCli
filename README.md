master | Project
--- | ---
[![Build status](https://ci.appveyor.com/api/projects/status/bgyngfdyprx8wqgp/branch/master?svg=true)](https://ci.appveyor.com/project/Pyrite/pyritecli/branch/master) | [![Build status](https://ci.appveyor.com/api/projects/status/bgyngfdyprx8wqgp?svg=true)](https://ci.appveyor.com/project/Pyrite/pyritecli)

# Cuber
### A tool for slicing meshes

*Cuber is under heavy development as part of a pre-processing pipeline for CubeServer.  You are welcome to use it now, but you should expect issues*

#### Features

* Parses Wavefront OBJ formatted mesh files
* Slices source mesh in three dimensions to create a set of smaller, valid OBJ files of any size
* Leverages all available CPU cores
* Works on very large meshes (tens of millions of vertices)
* Support for a custom binary output format (.ebo) designed for streaming mesh data
* Various miscellaneous features to support CubeServer



#### Limitations
* Currently supports V, VT, F, and MTLLIB commands in OBJ files
* Only tested on meshes with triangular faces



#### Usage
```cuber --help```
