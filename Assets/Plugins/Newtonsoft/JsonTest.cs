using Newtonsoft;
using Newtonsoft.Json;
using UnityEngine;

public class JsonTest : MonoBehaviour {
	void Start() {
		string json = "{\"name\":\"Karl\",\"age\":23}";
		Person p = JsonConvert.DeserializeObject<Person>(json);
		Debug.Log($"Name: {p.name}, Age: {p.age}");
	}

	class Person {
		public string name;
		public int age;
	}
}
