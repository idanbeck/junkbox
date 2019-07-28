" Launch Config
" call pathogen#infect()                      " use pathogen
" call pathogen#runtime_append_all_bundles()  " use pathogen

" Colors
colorscheme desert 

" syntax
syntax enable

" Tabs
set tabstop=4
set softtabstop=4
"set expandtabs

" UI Config
set number
set showcmd
set cursorline
filetype indent on
set wildmenu
set lazyredraw
set showmatch

" Search
set incsearch
set hlsearch

" Allows cursor change in tmux mode
if exists('$TMUX')
    let &t_SI = "\<Esc>Ptmux;\<Esc>\<Esc>]50;CursorShape=1\x7\<Esc>\\"
    let &t_EI = "\<Esc>Ptmux;\<Esc>\<Esc>]50;CursorShape=0\x7\<Esc>\\"
else
    let &t_SI = "\<Esc>]50;CursorShape=1\x7"
    let &t_EI = "\<Esc>]50;CursorShape=0\x7"
endif

