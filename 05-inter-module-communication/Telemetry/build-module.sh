#!/bin/bash

dotnet clean
dotnet restore
dotnet build
dotnet publish
