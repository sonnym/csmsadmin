#!/bin/bash
gmcs -t:library -optimize -warnaserror --fatal -r:System.Web.dll,System.Data.dll src/app_code/* -out:src/bin/csmsadmin.dll

