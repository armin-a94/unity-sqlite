using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using SQLite;

public class SQLiteDemo : MonoBehaviour
{

    [SerializeField] private string _databaseName = "test.db";
    
    public class TestAccount
    {
        [AutoIncrement, PrimaryKey] public long id { get; set; }
        [MaxLength(60)] public string name { get; set; }
        public int age { get; set; }
        public DateTime last_seen { get; set; }
    }
    
    private void Start()
    {
        SQLiteManager.OnDatabaseCreated += OnDatabaseCreated;
        SQLiteManager.CreateDatabaseFile(_databaseName, false);
        Debug.Log("Press 1 to insert random record. Press 2 to select random record. Press 3 to delete all records.");
    }

    private void OnDatabaseCreated(string databaseName)
    {
        if (databaseName == _databaseName)
        {
            var connection = SQLiteManager.GetConnection(databaseName);
            connection.CreateTableAsync<TestAccount>();
        }
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            StartCoroutine(InsertRandomAccount());
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            StartCoroutine(SelectRandomAccount());
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            StartCoroutine(DeleteAllAccounts());
        }
    }
    
    private IEnumerator InsertRandomAccount() => UniTask.ToCoroutine(async () =>
    {
        var connection = SQLiteManager.GetConnection(_databaseName);
        
        TestAccount account = new TestAccount();
        account.name = "Player" + UnityEngine.Random.Range(1, 99999).ToString();
        account.age = UnityEngine.Random.Range(20, 90);
        account.last_seen = DateTime.Now;
        
        await connection.InsertAsync(account);
        Debug.Log("Account created: ID = " + account.id.ToString());
    });
    
    private IEnumerator SelectRandomAccount() => UniTask.ToCoroutine(async () =>
    {
        var connection = SQLiteManager.GetConnection(_databaseName);
        // TestAccount account = await connection.FindAsync<TestAccount>(id);
        TestAccount account = await connection.FindWithQueryAsync<TestAccount>("SELECT * FROM TestAccount ORDER BY RANDOM() LIMIT 1");
        if (account != null)
        {
            Debug.Log("Account " + account.name + " selected: ID = " + account.id.ToString());
        }
        else
        {
            Debug.Log("There is no account to select.");
        }
    });
    
    private IEnumerator DeleteAllAccounts() => UniTask.ToCoroutine(async () =>
    {
        var connection = SQLiteManager.GetConnection(_databaseName);
        await connection.DeleteAllAsync<TestAccount>();
        Debug.Log("All accounts has been deleted.");
    });
    
}