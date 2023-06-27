# **DbDiffChecker**

This app is a tool for checking tables data and design differences between given databases (I am calling it for now uat and production). If you wanna test it built the code and run from built folder and app will listen 5000 (not ssl) and 5001 (ssl) port.

# Appsettings

appsettings.json has `CurrentConnectionString` key. if this key value is 
  *  `ConnectionString_UAT` then your sql files or reports will generate for production (can be executed in Prod database)
  *  `ConnectionString_Prod` then your sql files or reports will generate for uat (can be executed in uat database)
 
# Installation
After you run your app if you didn't set your connection strings in your `appsettings.json` then app will redirect you to installation page. You need to fill your connectionstring values for uat and production correctly otherwise app can't login it will throw error.

![Installation Page](https://i.imgur.com/LIvcQ1W.png) 

# Usage
After installation click `UAT-Production Db Design Differences` on navlink and start to use app.
