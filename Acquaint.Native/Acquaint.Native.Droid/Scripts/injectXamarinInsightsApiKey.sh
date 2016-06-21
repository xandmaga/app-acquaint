#!/bin/bash

sed -i '' "s/Insights.DebugModeKey/\"$2\"/g" "$1/MainApplication.cs"