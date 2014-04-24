using System;
using System.Xml;

namespace MazeEditor
{
	/// <summary>
	/// Summary description for XmlHelper.
	/// </summary>
	public abstract class XmlHelper
	{

		public static double GetDoubleAttributeFromNode(XmlNode node, string  attributeName)
		{
			if (node.Attributes.GetNamedItem(attributeName) == null)
			{
				throw new Exception(string.Concat("Numerical attribute missing: ",node.Name," -> ",attributeName,"\nat node: \n",node.OuterXml));
			}
			try
			{
				return XmlConvert.ToDouble(node.Attributes.GetNamedItem(attributeName).Value);
			}
			catch 
			{
				throw new Exception(string.Concat("Numerical attribute error: ",node.Name," ." ,attributeName," = ",node.Attributes.GetNamedItem(attributeName).Value,"\nat node: \n",node.OuterXml));
			}

		}//GetDoubleAttributeFromNode


		public static double GetDoubleAttributeFromNode(XmlNode node, string  attributeName,double defaultValue)
		{
			if (node.Attributes.GetNamedItem(attributeName) == null)
			{
				return defaultValue;
			}
			try
			{
				return XmlConvert.ToDouble(node.Attributes.GetNamedItem(attributeName).Value);
			}
			catch 
			{
				throw new Exception(string.Concat("Numerical attribute error: ",node.Name," ." ,attributeName," = ",node.Attributes.GetNamedItem(attributeName).Value,"\nat node: \n",node.OuterXml));
			}

		}//GetDoubleAttributeFromNode
		
		public static string GetStringAttributeFromNode(XmlNode node, string  attributeName)
		{
			if (node.Attributes.GetNamedItem(attributeName) == null)
			{
				throw new Exception(string.Concat("string attribute missing: ",node.Name," -> ",attributeName,"\nat node: \n",node.OuterXml));
			}
			try
			{
				if (node.Attributes.GetNamedItem(attributeName).Value.Length != 0)
				{
					return node.Attributes.GetNamedItem(attributeName).Value;
				}
				else
				{
					throw new Exception();
				}
			}
			catch 
			{
				throw new Exception(string.Concat("string attribute error: ",node.Name," -> ",attributeName," = ",node.Attributes.GetNamedItem(attributeName).Value,"\nat node: \n",node.OuterXml));
			}
		}//GetStringAttributeFromNode
		
		public static string GetStringAttributeFromNode(XmlNode node, string  attributeName, string defaultValue)
		{
			if (node.Attributes.GetNamedItem(attributeName) == null)
			{
				return defaultValue;
			}
			try
			{
				if (node.Attributes.GetNamedItem(attributeName).Value.Length != 0)
				{
					return node.Attributes.GetNamedItem(attributeName).Value;
				}
				else
				{
					throw new Exception();
				}
			}
			catch 
			{
				throw new Exception(string.Concat("string attribute error: ",node.Name," -> ",attributeName," = ",node.Attributes.GetNamedItem(attributeName).Value,"\nat node: \n",node.OuterXml));
			}
		}//GetStringAttributeFromNode
	
	
	
	}
}
