using UnityEngine;
using System;
using UnityEngine.UI;

public class Test01: MonoBehaviour
{
	public InputField m_input;
	public Text m_text;
	
	static void Main(string []args)
	{
		
	}
	
	public void Show_text()
	{
		m_text.text = m_input.text;
	}
}