#!/bin/bash

sed -i '' "s/HOCKEYAPP_APPID/$2/g" "$1/MainApplication.cs"