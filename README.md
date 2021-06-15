# WeakSum
WeakSum is a very simple and quick checksum algorithm that was made for fun. It is not intended for any security applications and should most definetly not be used for passwords in any way.

## Recommended usage
Either if you know what you are doing and want to improve the project or if you want to play around with it just for fun.

## Usage
On Linux Mono is required to be able to run WeakSum. After installing Mono all following commands should start with `mono WeakSum.exe` instead of `.\WeakSum.exe`.

To get the weaksum of a file run `.\WeakSum.exe <File-Path>`.
To get the weaksum of a string run `.\WeakSum.exe "<String-Goes-Here>"`, make sure that the string is formatted correctly to not accidentally make it into two arguments.
To get the weaksum of a string, but with a dialog just run `.\WeakSum.exe` and you should be prompted by a dialog.
