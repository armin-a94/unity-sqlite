using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SQLite;
using System;
using Cysharp.Threading.Tasks;

public class AuthenticationDemo : MonoBehaviour
{
 
    [SerializeField] private string _databaseName = "authentication_demo.db";
    private bool _initialized = false;
    public delegate void Callback(Account account);
    
    public class Account
    {
        [AutoIncrement, PrimaryKey] public long id { get; set; }
        [MaxLength(60)] public string name { get; set; }
    }
    
    public class Authentication
    {
        public long account_id { get; set; }
        [MaxLength(100)] public string token { get; set; }
        [MaxLength(200)] public string key { get; set; }
        [MaxLength(100)] public string method { get; set; }
        public DateTime last_auth { get; set; }
    }
    
    private void Initialize()
    {
        if (_initialized)
        {
            return;
        }
        _initialized = true;
        if (SQLiteManager.CreateDatabaseFile(_databaseName, false, "template.db"))
        {
            var connection = SQLiteManager.GetConnection(_databaseName);
            connection.CreateTable<Account>();
            connection.CreateTable<Authentication>();
        }
    }
    
    private void Start()
    {
        Initialize();
    }

    public Account Authenticate()
    {
        string token = SystemInfo.deviceUniqueIdentifier;
        string key = SQLiteTools.MD5Hash(SystemInfo.processorType);
        return Authenticate(token, key, "anonymous", true);
    }
    
    public Account Authenticate(string token, string key, string method, bool createAccountIfNotExists)
    {
        Initialize();
        var connection = SQLiteManager.GetConnection(_databaseName);
        var auth = connection.FindWithQuery<Authentication>("SELECT * FROM Authentication WHERE token = ? AND key = ? AND method = ?", token, key, method);
        if (auth != null)
        {
            connection.Query<Authentication>("UPDATE Authentication SET last_auth = CURRENT_TIMESTAMP WHERE token = ? AND key = ? AND method = ?", token, key, method);
            return connection.Find<Account>(auth.account_id);
        }
        else
        {
            if (createAccountIfNotExists)
            {
                Account account = new Account();
                account.name = "Player" + UnityEngine.Random.Range(1, 99999).ToString();
                connection.Insert(account);

                Authentication authentication = new Authentication();
                authentication.account_id = account.id;
                authentication.token = token;
                authentication.key = key;
                authentication.method = method;
                authentication.last_auth = DateTime.Now;
                connection.Insert(authentication);

                return account;
            }
        }
        return null;
    }
    
    public void AuthenticateAsync(Callback callback)
    {
        string token = SystemInfo.deviceUniqueIdentifier;
        string key = SQLiteTools.MD5Hash(SystemInfo.processorType);
        AuthenticateAsync(token, key, "anonymous", true, callback);
    }
    
    public void AuthenticateAsync(string token, string key, string method, bool createAccountIfNotExists, Callback callback)
    {
        StartCoroutine(AuthenticateAsyncTask(token, key, method, createAccountIfNotExists, callback));
    }
    
    private IEnumerator AuthenticateAsyncTask(string token, string key, string method, bool createAccountIfNotExists, Callback callback) => UniTask.ToCoroutine(async () =>
    {
        Initialize();
        var connection = SQLiteManager.GetAsyncConnection(_databaseName);
        var auth = await connection.FindWithQueryAsync<Authentication>("SELECT * FROM Authentication WHERE token = ? AND key = ? AND method = ?", token, key, method);
        if (auth != null)
        {
            await connection.QueryAsync<Authentication>("UPDATE Authentication SET last_auth = CURRENT_TIMESTAMP WHERE token = ? AND key = ? AND method = ?", token, key, method);
            var account = await connection.FindAsync<Account>(auth.account_id);
            callback.Invoke(account);
        }
        else
        {
            if (createAccountIfNotExists)
            {
                Account account = new Account();
                account.name = "Player" + UnityEngine.Random.Range(1, 99999).ToString();
                await connection.InsertAsync(account);

                Authentication authentication = new Authentication();
                authentication.account_id = account.id;
                authentication.token = token;
                authentication.key = key;
                authentication.method = method;
                authentication.last_auth = DateTime.Now;
                await connection.InsertAsync(authentication);

                callback.Invoke(account);
            }
            else
            {
                callback.Invoke(null);
            }
        }
    });
    
}