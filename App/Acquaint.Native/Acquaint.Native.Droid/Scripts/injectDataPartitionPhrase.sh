#!/bin/bash

sed -i '' "s/DataPartitionPhrase/$2/g" "$1/UpdateDataTests.cs"