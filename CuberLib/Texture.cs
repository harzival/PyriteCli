﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using af = AForge.Imaging;
using CuberLib.Types;

namespace CuberLib
{
	public class Texture
	{
		private Obj obj;
		private Bitmap[,] texture;

		public Texture(Obj obj)
		{
			this.obj = obj;
		}

		public void NaiveTriangleRepack(string texturePath, string outputPath)
		{
			
			List<Face> chunkFaceList;
			chunkFaceList = obj.FaceList.AsParallel().Where(v => v.InExtent(
				new Extent()
				{
					XMax = obj.Size.XMax - (obj.Size.XSize / 2),
					YMax = obj.Size.YMax - (obj.Size.YSize / 2),
					ZMax = obj.Size.ZMax,
					XMin = obj.Size.XMin,
					YMin = obj.Size.YMin,
					ZMin = obj.Size.ZMin
				}, obj.VertexList)).ToList();

			// get list of UV triangles (maybe not distinct?)
			var triangles = chunkFaceList.Select(f => new Tuple<TextureVertex, TextureVertex, TextureVertex>(
				obj.TextureList[f.TextureVertexIndexList[0] - 1],
				obj.TextureList[f.TextureVertexIndexList[1] - 1],
				obj.TextureList[f.TextureVertexIndexList[2] - 1])).ToList();

			// Load original texture and initiate new texture
			Image original = Image.FromFile(texturePath);
			Bitmap output = new Bitmap(original.Width, original.Height);
			using (Graphics destGraphics = Graphics.FromImage(output))
			{

				// write into same location in new bitmap
				for (int i = 0; i < triangles.Count; i++)
				{
					var triangle = triangles[i];
					var poly = new PointF[] {
					new PointF((float)(triangle.Item1.X * original.Width), (float)((1-triangle.Item1.Y) * original.Height)),
					new PointF((float)(triangle.Item2.X * original.Width), (float)((1-triangle.Item2.Y) * original.Height)),
					new PointF((float)(triangle.Item3.X * original.Width), (float)((1-triangle.Item3.Y) * original.Height)),
					new PointF((float)(triangle.Item1.X * original.Width), (float)((1-triangle.Item1.Y) * original.Height))
					};

					CopyPolygon(original, destGraphics, poly, poly);

					if (i % 10000 == 0)
					{
						Console.WriteLine("{0} of {1}", i, triangles.Count);
					}
				}

				destGraphics.ResetClip();		
			}

			// Identify blob rectangles

			af.BlobCounter bc = new af.BlobCounter();
			bc.ProcessImage(output);
			Rectangle[] sourceRects = bc.GetObjectsRectangles();
			Rectangle[] destRects = PackTextures(sourceRects, 1024, 1024, 8192);

			int width = destRects.Max<Rectangle, int>(r => r.X + r.Width);
			int height = destRects.Max<Rectangle, int>(r => r.Y + r.Height);

			Bitmap packed = new Bitmap(width, height);

			using (Graphics packedGraphics = Graphics.FromImage(packed))
			{
				for (int i = 0; i < sourceRects.Length; i++)
				{
					packedGraphics.DrawImage(output, destRects[i], sourceRects[i], GraphicsUnit.Pixel);
				}
			}

			// write output files

			string path = Path.Combine(outputPath, "output.jpg");
			if (File.Exists(path)) File.Delete(path);
            packed.Save(path, ImageFormat.Jpeg);

			packed.Dispose();
			output.Dispose();
			original.Dispose();
		}

		private Rectangle[] PackTextures(Rectangle[] source, int width, int height, int maxSize)
		{
			if (width > maxSize || height > maxSize) return null;			

			MaxRectanglesBinPack bp = new MaxRectanglesBinPack(width, height, false);
			Rectangle[] rects = new Rectangle[source.Length];

			for (int i = 0; i < source.Length; i++)
			{
				Rectangle rect = bp.Insert(source[i].Width, source[i].Height, MaxRectanglesBinPack.FreeRectangleChoiceHeuristic.RectangleBestAreaFit);
				if (rect.Width == 0 || rect.Height == 0)
				{
					return PackTextures(source, width * (width <= height ? 2 : 1), height * (height < width ? 2 : 1), maxSize);
				}
				rects[i] = rect;
			}

			return rects;
		}


		private void CopyPolygon(Image source, Graphics dest, PointF[] sourcePoly, PointF[] destPoly)
		{
			using (GraphicsPath gpdest = new GraphicsPath())
			using (GraphicsPath gpdestWide = new GraphicsPath())
			{
				gpdest.AddPolygon(destPoly);
				gpdestWide.AddPolygon(destPoly);
				gpdestWide.Widen(Pens.Black);

				//Draw on the Bitmap
				dest.SetClip(gpdest);
				dest.SetClip(gpdestWide, CombineMode.Union);
				dest.DrawImage(source, 0, 0);
			}
		}
	}
}
