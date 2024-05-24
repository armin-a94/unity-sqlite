using System;
using System.Collections.Generic;
using SQLite;
using System.IO;
using UnityEngine;

public class SQLiteManager : MonoBehaviour
{
    
    private static SQLiteManager _singleton = null; public static SQLiteManager singleton { get { if (_singleton == null) { _singleton = FindFirstObjectByType<SQLiteManager>(); if (_singleton == null) { _singleton = new GameObject("SQLiteManager").AddComponent<SQLiteManager>(); } _singleton.Initialize(); } return _singleton; } }
    private bool _initialized = false;
    private HashSet<string> _databases = new HashSet<string>();
    private string _directory = "";
    public delegate void DatabaseDelegate(string databaseName);
    public static event DatabaseDelegate OnDatabaseCreated;
    
    public virtual void Awake()
    {
        if (_singleton != null && _singleton != this)
        {
            Destroy(this);
        }
        else
        {
            Initialize();
        }
    }

    private void Initialize()
    {
        if (_initialized) { return; }
        DontDestroyOnLoad(gameObject);
        SQLiteConnectionPool.Shared.Reset();
        _directory = Path.Combine(Application.persistentDataPath, "Database");
        _initialized = true;
    }

    public static SQLiteAsyncConnection GetAsyncConnection(string databaseName)
    {
        return singleton._GetAsyncConnection(databaseName);
    }
    
    private SQLiteAsyncConnection _GetAsyncConnection(string databaseName)
    {
        if(!_databases.Contains(databaseName))
        {
            CreateDatabaseFile(databaseName, false);
            if (_IsDatabaseFileExists(databaseName))
            {
                _databases.Add(databaseName);
            }
            else
            {
                return null;
            }
        }
        return new SQLiteAsyncConnection(Path.Combine(_directory, databaseName));
    }
    
    public static SQLiteConnection GetConnection(string databaseName)
    {
        return singleton._GetConnection(databaseName);
    }
    
    private SQLiteConnection _GetConnection(string databaseName)
    {
        if(!_databases.Contains(databaseName))
        {
            CreateDatabaseFile(databaseName, false);
            if (_IsDatabaseFileExists(databaseName))
            {
                _databases.Add(databaseName);
            }
            else
            {
                return null;
            }
        }
        return new SQLiteConnection(Path.Combine(_directory, databaseName));
    }
    
    public static bool CreateDatabaseFile(string databaseName, bool overwriteIfExists, string templateDatabaseName = "template.db")
    {
        return singleton._CreateDatabaseFile(databaseName, overwriteIfExists, templateDatabaseName);
    }
    
    private bool _CreateDatabaseFile(string databaseName, bool overwriteIfExists, string templateDatabaseName = "template.db")
    {
        if (!Directory.Exists(_directory))
        { 
            Directory.CreateDirectory(_directory);
        }
        string _path = Path.Combine(_directory, databaseName);
        if (!File.Exists(_path) || overwriteIfExists)
        {
            var templatePath = Path.Combine(Application.streamingAssetsPath, templateDatabaseName);
            if (File.Exists(templatePath))
            {
                if (File.Exists(_path))
                {
                    File.Delete(_path);
                }
                File.WriteAllBytes(_path, File.ReadAllBytes(templatePath));
                if (OnDatabaseCreated != null)
                {
                    OnDatabaseCreated.Invoke(databaseName);
                }
                return true;
            }
        }
        return false;
    }
    
    public static bool IsDatabaseFileExists(string databaseName)
    {
        return singleton._IsDatabaseFileExists(databaseName);
    }
    
    private bool _IsDatabaseFileExists(string databaseName)
    {
        return File.Exists(Path.Combine(_directory, databaseName));
    }
    
    public static string BackupDatabase(string databaseName)
    {
        return singleton._BackupDatabase(databaseName);
    }
    
    private string _BackupDatabase(string databaseName)
    {
        string backupName = "";
        var directory = Path.Combine(Application.persistentDataPath, "Database");
        var databasePath = Path.Combine(directory, databaseName);
        if (File.Exists(databasePath))
        {
            backupName = databaseName + "_" + DateTime.Now.ToString("yyyyMMddTHHmmss");
            var newPath = Path.Combine(directory, backupName);
            File.WriteAllBytes(newPath, File.ReadAllBytes(databasePath));
        }
        return backupName;
    }
    
}