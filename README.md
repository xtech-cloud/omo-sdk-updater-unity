# 清单组成

清单是一份包含所有资源文件的描述的数组。资源文件的描述包括

- uuid

资源文件的唯一识别码

- path

资源文件的存储路径

- file

资源文件的文件名

- md5

资源文件的MD5值

- size

资源文件的大小


# 更新流程

Updater从服务器拉取清单过后，会查找本地是否有资源对应的md5文件，如果没找到md5文件或者md5文件中的值和资源文件的md5值不一致，则会将资源加入到下载列表中。

`注意：检测时并不会对比本地文件的实际MD5值，updater假设资源从上次下载完成，生成MD5文件后，资源文件本身并没有发生变化。`