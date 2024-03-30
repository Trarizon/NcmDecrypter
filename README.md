# NcmConverter

转换ncm文件为mp3

基于.NET8编写，需要[.NET8.0运行时](https://dotnet.microsoft.com/zh-cn/download/dotnet/8.0)

转换时保留了ncm文件中的Comment，网易云客户端可直接识别转换后的mp3文件（记得清缓存

## 使用

输入格式：`[-nc] [paths..]`
- `-nc` 排除音乐mp3文件内的封面
- `paths` ncm文件路径，可以同时输入多个

支持直接拖动到exe文件上运行

## 项目

`NcmDecrypter`包含主要的解密逻辑。
（源码提供了不保留Comment的选项，但是用户交互里我没写qwq

`NcmDecrypter.Execute`为控制台程序

## 参考

[NCM2MP3 by charlotte-xiao](https://github.com/charlotte-xiao/NCM2MP3)
