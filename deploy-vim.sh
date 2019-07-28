# Deploy the VIM RC configuration and install
# external dependencies as needed

if [ -f ~/.vimrc ] ; then
    cp ~/.vimrc .vimrc.orig
fi

cp ./vimdir/.vimrc ~/.vimrc

 




