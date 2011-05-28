using System.Windows;
using Exolutio.ViewToolkit;

namespace XCase.WPFDraw.Geometries
{
	public class JunctionGeometryData
	{
		public Connector Connector { get; private set; }

        public JunctionGeometryData(Connector connector)
		{
			Connector = connector;
		}
	}
}