# NcmConverter

因为想找个方便本地用的转换器所以写了

具体参考了[@charlotte-xiao的这个库](https://github.com/charlotte-xiao/NCM2MP3)。
加密过程写的很详细，帮大忙了，所以我就不写了（

但是很可惜我不用java所以干脆重写了（

*Windows Terminal不认有的字符比如`♪`，可以拖动执行*

## 项目构成

`NcmDecrypter.Core`包含主要的解密逻辑

`NcmDecrypter`是控制台程序，`Utility`里包含了设置metadata的代码，这里有用到`TagLibSharp`包。
顺带一提mp3自带不少信息，所以metadata主要是图片。

`NcmDecrypter.Light`是简化版程序，去掉了包依赖，也没法设置metadata。（一般来说就够了，我不想这玩意还要拖着个dll

## 使用说明

可以直接把ncm拖到exe上运行，可以拖入多个。

标准的输入为`[file...] [-l [file...]]`，`-l`以后的所有文件不会设置metadata。

NcmDecrypter.Light不含`-l`功能，一律不设置metadata
