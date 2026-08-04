using JinGroup.Base.LoadData;
using UnityEngine;
using UnityEngine.Purchasing.Security;

[CreateAssetMenu(fileName = "DataPurchase", menuName = "Data/DataPurchase")]
public class DataPurchase : ScriptableObject
{
    public string GoogleBase64String =
        "TIp6JEbH8OiXyJ/AtU2h0ikwCJCG+lt/EPAstS22n+H8xKcoLuqo+1UyArZWt22+MM8hPXNHw+IhA1vLK3OSzu1AkbVBp/jcE7FDb4m0X77VyMaOWdVr8n6s0SbckHpUXizm0l1cNIE+0/BAL2GISWouKnevEZXAywxKuLxyXzLaVXmq9bhByd11vbrv4/85Mym3HjQqGSeL6sHb/yMGhEt/nYjhfEgytiMbWwQ4IhvJsheRo7JBwKS2B4fwnZzuGnbwrn7xprVDx0HPKku1n8rnA/9CXfY/yNDe/q8dnr2vkpmWtRnXGWiSnp6emp+cHZ6Qn68dnpWdHZ6enwKphzJXz5aW/Rkh+vlkESXLE1CsoXM2SS3HHkO7BZrp5YMVjJ2cnp+e";

    public int[] GoogleOrder = new int[] { 4, 10, 12, 12, 12, 8, 7, 13, 11, 13, 11, 12, 12, 13, 14 };
    public int   Googlekey   = 159;

    public string AppleBase64String =
        "BqpkqtshLS0pKSwcTh0nHCUqL3lVDE1fX1lBSV8MTU9PSVxYTUJPSTOpr6k3tRFrG96Ft2yiAPidvD70TkBJDF9YTUJITV5IDFhJXkFfDE1l9FqzHzhJjVu45QEuLy0sLY+uLa4tLColBqpkqttPSCktHK3eHAYqKCo/Lnl/HT8cPSoveSgmPyZtXFwhKiUGqmSq2yEtLSkpLC+uLS0scCQHKi0pKSsuLToyRFhYXF8WAwNbXEBJDH5DQ1gMb20cMjshHBocGB4jsRHfB2UENuTS4pmVIvVyMPrnERq1YAFUm8Ggt/DfW7feWv5bHGPt5TVe2XEi+VNzt94JL5Z5o2FxId2ZFoHYIyIsviedDToCWPkQIfdOOrmyViCIa6d3+DobH+foI2HiOEX9EQpLDKYfRtshruPyx48D1X9Gd0iS2F+3wv5II+dVYxj0jhLVVNNH5AxNQkgMT0leWEVKRU9NWEVDQgxcU22EtNX95kqwCEc9/I+XyDcG7zMCbIrba2FTJHIcMyoveTEPKDQcOioveTEiKDooOAf8RWu4WiXS2EehhPBSDhnmCfn1I/pH+I4IDz3bjYAIzsf9m1zzI2nNC+bdQVTBy5k7O35JQEVNQk9JDENCDFhERV8MT0leKSwvri0jLByuLSYuri0tLMi9hSVeTU9YRU9JDF9YTVhJQUlCWF8CHHWLKSVQO2x6PTJY/5unDxdrj/lDXEBJDG9JXlhFSkVPTVhFQ0IMbVkDHK3vKiQHKi0pKSsuLhytmjatnwocCCoveSgnPzFtXFxASQxvSV5YrDgH/EVruFol0thHoQJsittrYVMfGnYcTh0nHCUqL3koKj8ueX8dP0JIDE9DQkhFWEVDQl8MQ0oMWV9JS6MkmAzb54AADENcmhMtHKCbb+McriiXHK4vj4wvLi0uLi0uHCEqJQxDSgxYREkMWERJQgxNXFxARU9N9RpT7at59Yu1lR5u1/T5XbJSjX4cPSoveSgmPyZtXFxASQxlQk8CHac1pfLVZ0DZK4cOHC7ENBLUfCX/QEkMZUJPAh0KHAgqL3koJz8xbVxFSkVPTVhFQ0IMbVlYRENeRVhVHUgZDzlnOXUxn7jb2rCy43yW7XR8AAxPSV5YRUpFT01YSQxcQ0BFT1U6HDgqL3koLz8hbVxcQEkMfkNDWKNfrUzqN3clA76e1Ghk3EwUsjnZVhyuLVocIioveTEjLS3TKCgvLi1bWwJNXFxASQJPQ0EDTVxcQElPTZ0cdMB2KB6gRJ+jMfJJX9NLckmQGR4dGBwfGnY7IR8ZHB4cFR4dGBwkchyuLT0qL3kxDCiuLSQcri0oHCvAURWvp38M/xTonZO2YyZH0wfQM733Mmt8xynBclWoAccajntgecDsTx9b2xYrAHrH9iMNIvaWXzVjmQxvbRyuLQ4cISolBqpkqtshLS0tmzeRv24IPgbrIzGaYbByT+RnrDtYRUpFT01YSQxOVQxNQlUMXE1eWIePXb5rf3ntgwNtn9TXz1zhyo9gaVIzYEd8um2l6FhOJzyvbasfpq1YRENeRVhVHTocOCoveSgvPyFtXCocIyoveTE/LS3TKCkcLy0t0xwxfIam+fbI0PwlKxucWVkN";

    public int[] AppleOrder = new int[]
    {
        1, 40, 49, 42, 27, 34, 49, 47, 35, 49, 22, 48, 19, 23, 18, 56, 50, 45, 45, 31, 30, 54, 53, 38, 45, 46, 52,
        27, 37, 53, 32, 35, 43, 54, 40, 41, 58, 49, 50, 42, 48, 43, 44, 43, 55, 49, 58, 52, 52, 54, 51, 56, 57, 56,
        59, 55, 59, 58, 58, 59, 60
    };

    public int Applekey = 44;

    public static readonly bool IsPopulated = true;

    public byte[] GoogleData()
    {
        return IsPopulated == false ? null : Obfuscator.DeObfuscate(System.Convert.FromBase64String(GoogleBase64String), GoogleOrder, Googlekey);
    }

    public byte[] AppleData()
    {
        return IsPopulated == false ? null : Obfuscator.DeObfuscate(System.Convert.FromBase64String(AppleBase64String), AppleOrder, Applekey);
    }
    
}