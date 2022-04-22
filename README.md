# UnityNeo4jExampleV1


PreSetup
	In neo4j execute the following to create the cube and its stored data:
		CREATE (n:Entity {name: 'TestCube', Speed: 5})
	To edit this data execute:
		MATCH (p:Entity) WHERE p.name = "TestCube" Set p.Speed = (the number you want)


Setup instructions:
	Launch the Neo4jExample, located in the github download (or right above this file).
	Open up the sample scene if it didnt.
	Got to the neo4jManager and for the External server path, put in the path to the exe file in the folder called: External Server
	It should look somthing like this : C:\Downloads\UnityNeo4jExampleV1\ExternalServer\Neo4J_CSharp.exe
	Fill out the rest or the fields in the neo4j Manager.
	Click play.