// Note:
// This script must be executed before Dif or DifResource is used.

using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
public class DifResourceManager : MonoBehaviour {

	static List<DifResource> resources = new List<DifResource>();
	
	/// <summary>
	/// Gets or creates a DIF resource.
	/// </summary>
	/// <param name="difPath">The path to the dif file.</param>
	/// <returns></returns>
	public static DifResource getResource(string difPath) {
		foreach (var i in resources) {
			if (i.file == difPath) {
				return i;
			}
		}

		// it doesn't exist, create one!
		var resource = new DifResource(difPath);
		resources.Add(resource);
		return resource;
	}
}
