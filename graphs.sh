#!/bin/bash

# Create the directory that will contain the dotfiles and graphs, in case that
# it doesn't exist.
graphs_dir=$(jq -r .graphsDirectory config.json)
mkdir -p $graphs_dir

if [ "$1" == "compile" ]; then
    if [ -d "$graphs_dir" ] && [ -z "$(ls -A $graphs_dir    )" ]; then
        echo "No graph files to be compiled."
    else
        # Use the `ls` command to get a list of all files in the directory with the
        # specified extension.
        for file in $(ls ./$graphs_dir/*.dot); do
            # Get the base file name without the extension.
            base=$(basename "$file" .dot)
            # Execute the command and redirect the output to a new file with the same name
            dot -Tpng -o "$graphs_dir/$base.png" "$graphs_dir/$file"
        done
    fi
elif [ "$1" == "clean" ]; then
    # Deleting all graph files.
    if [ -d "$graphs_dir" ] && [ -z "$(ls -A $graphs_dir    )" ]; then
        echo "No graph files to be deleted."
    else
        rm $graphs_dir/*
    fi
fi
