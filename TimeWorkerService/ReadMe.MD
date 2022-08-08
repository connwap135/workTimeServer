# 记录一次树莓派运行.net5 应用
### 1 更换软件更新源
sudo nano /etc/apt/sources.list
```
deb http://mirrors.tuna.tsinghua.edu.cn/raspbian/raspbian/ stretch main contrib non-free rpi
deb-src http://mirrors.tuna.tsinghua.edu.cn/raspbian/raspbian/ stretch main contrib non-free rpi
```
### 2 更换系统更新源
sudo nano /etc/apt/sources.list.d/raspi.list
```
deb http://mirror.tuna.tsinghua.edu.cn/raspberrypi/ stretch main ui
deb-src http://mirror.tuna.tsinghua.edu.cn/raspberrypi/ stretch main ui
```
sudo apt-get update
sudo apt-get upgrade
sudo apt-get dist-upgrade

### 3 安装MySQL
sudo apt-get install mysql-server
sudo mysql -u root -p
空密码
```
use mysql;

# 修改密码为123456
set password=PASSWORD('123456');
# 添加远程访问
GRANT ALL PRIVILEGES ON *.* TO 'root'@'%' IDENTIFIED BY '123456';
# 刷新
flush privileges;

# 退出
exit;
```
sudo nano /etc/mysql/mariadb.conf.d/50-server.cnf

修改bind-address   = 127.0.0.1  改成 0.0.0.0
```
# Instead of skip-networking the default is now to listen only on
# localhost which is more compatible and is not less secure.
bind-address		= 0.0.0.0
```
### 4 安装net5.0本地编译 或发布单文件应用拷贝到指定目录
```
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin -c 5.0
```
编辑环境变量

sudo nano ~/.bashrc
```
export DOTNET_ROOT=$HOME/.dotnet
export PATH=$PATH:$HOME/.dotnet
```
刷新环境变量

source ~/.bashrc

拷贝应用至/home/pi/app

### 5 安装supervisor
```
sudo apt-get install supervisor

mkdir /etc/supervisord.d

cd /etc/supervisord.d
```
创建supervisor应用

sudo nano app1.conf
```
[program:app1]
directory = /home/pi/app
command = /home/pi/app/app1
autostart = true
startsecs = 5
user = pi
redirect_stderr = true
stdout_logfile = /home/pi/app/app1.log
```
修改配置
sudo nano /etc/supervisord.conf
```
[include]
files = /etc/supervisord.d/*.conf
```
supervisord 管理
Supervisord 安装完成后有两个可用的命令行 supervisord 和 supervisorctl，命令使用解释如下：

supervisord，初始启动 Supervisord，启动、管理配置中设置的进程。

supervisorctl stop app1，停止某一个进程(app1)

supervisorctl start app1，启动某个进程

supervisorctl restart app1，重启某个进程

supervisorctl stop groupworker: ，重启所有属于名为 groupworker 这个分组的进程(start,restart 同理)

supervisorctl stop all，停止全部进程，注：start、restart、stop 都不会载入最新的配置文件。

supervisorctl reload，载入最新的配置文件，停止原有进程并按新的配置启动、管理所有进程。

supervisorctl update，根据最新的配置文件，启动新配置或有改动的进程，配置没有改动的进程不会受影响而重启。

### 树莓派配置 内存卡   
sudo raspi-config

Advanced Options =>Expand Filesystem
