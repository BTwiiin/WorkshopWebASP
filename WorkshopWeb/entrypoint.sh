#!/bin/bash
set -e

# Run migrations
dotnet ef database update

# Start the main process
dotnet YourApplication.dll