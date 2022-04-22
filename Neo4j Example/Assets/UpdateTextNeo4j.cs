using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class UpdateTextNeo4j : MonoBehaviour
{
    Text text;
    PullNeo4j neo4J;
    void Start()
    {
        text = GetComponent<Text>();
        neo4J = PullNeo4j.instance;
    }
    void FixedUpdate()
    {
        text.text = neo4J.DataOut.ToString();
    }
}
