#!/usr/bin/env bash

template_path="$(pwd)/CSharp"

if [ "$1" = "u"  ]
then
    echo "Uninstalling $template_path"
    dotnet new -u "$template_path"
else
    echo "Installing $template_path"
    dotnet new -i "$template_path"
fi
