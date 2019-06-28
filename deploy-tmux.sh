# deploy tmux configuration and other details

if [ -f ~/.tmux.conf ] ; then
    cp ~/.tmux.conf ~/.tmux.conf.orig
fi

cp ./homedir/.tmux.conf ~/.tmux.conf 
