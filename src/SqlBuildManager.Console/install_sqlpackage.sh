
curl -L -o sqlpackage.zip https://go.microsoft.com/fwlink/?linkid=2134311
sudo apt-get install libunwind8
# install the libicu library based on the Ubuntu version
sudo apt-get install libicu60      # for 18.x
sudo apt install unzip
mkdir sqlpackage
unzip sqlpackage.zip -d /sqlpackage 
echo "export PATH=\"\$PATH:$HOME/sqlpackage\"" >> ~/.bashrc
chmod a+x ~/sqlpackage/sqlpackage
