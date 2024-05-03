using System;
using System.Collections.Generic;
using SQLite;
using System.IO;
using UnityEngine;

public class SQLiteManager : MonoBehaviour
{
    
    private string _templateDatabaseName = "template.db";
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

    public static SQLiteAsyncConnection GetConnection(string databaseName)
    {
        return singleton._GetConnection(databaseName);
    }
    
    private SQLiteAsyncConnection _GetConnection(string databaseName)
    {
        if(!_databases.Contains(databaseName))
        {
            bool created = CreateDatabaseFile(databaseName, false);
            if (created)
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
    
    public static bool CreateDatabaseFile(string databaseName, bool overwriteIfExists)
    {
        return singleton._GenerateDatabaseFile(databaseName, overwriteIfExists);
    }
    
    private bool _GenerateDatabaseFile(string databaseName, bool overwriteIfExists)
    {
        if (!Directory.Exists(_directory))
        { 
            Directory.CreateDirectory(_directory);
        }
        string _path = Path.Combine(_directory, databaseName);
        if (File.Exists(_path) && !overwriteIfExists)
        {
            return true;
        }
        var templatePath = Path.Combine(Application.streamingAssetsPath, _templateDatabaseName);
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
        return false;
    }
    
    public static void BackupDatabase(string databaseName)
    {
        singleton._BackupDatabase(databaseName);
    }
    
    private void _BackupDatabase(string databaseName)
    {
        // ToDo: Async
        var directory = Path.Combine(Application.persistentDataPath, "Database");
        var databasePath = Path.Combine(directory, databaseName);
        if (File.Exists(databasePath))
        {
            var newPath = Path.Combine(directory, databaseName + "_" + DateTime.Now.ToString("yyyyMMddTHHmmss"));
            File.WriteAllBytes(newPath, File.ReadAllBytes(databasePath));
        }
    }
    
}