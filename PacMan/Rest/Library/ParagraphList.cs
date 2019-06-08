using System;
using System.Collections.Generic;
using OpenTK;
using amulware.Graphics;

namespace PacMan
{
    public class ParagraphList
    {
        // Members
        /// <summary>
        /// The left margin of the paragraph text
        /// </summary>
        public int ParagraphOffsetX;
        /// <summary>
        /// The height of the text in pixels
        /// </summary>
        public int TextHeight;
        /// <summary>
        /// The extra space below the text
        /// </summary>
        public int TextMargin;
        /// <summary>
        /// The list of paragraphs
        /// </summary>
        public List<Paragraph> Text;
        /// <summary>
        /// The font for the paragraph text
        /// </summary>
        public FontGeometry FontGeometrie;
        /// <summary>
        /// The offset at which the paragraphlist should be drawn
        /// </summary>
        public Vector2 Position;
        /// <summary>
        /// The colour
        /// </summary>
        public Color Colour;

        // Properties
        public Paragraph this[int index] {
            get { return this.Text[index]; }
            set { this.Text[index] = value; }
        }
        public int Count {
            get { return this.Text.Count; }
        }

        // Methods
        public ParagraphList(FontGeometry fontGeo, Vector2 position, Color colour, int paragraphOffsetX = 10, int textHeight = 24, int textMargin = 4) {
            // Set all values
            this.Text = new List<Paragraph>();
            this.FontGeometrie = fontGeo;
            this.Position = position;
            this.Colour = colour;
            this.ParagraphOffsetX = paragraphOffsetX;
            this.TextHeight = textHeight;
            this.TextMargin = textMargin;
        }

        public void Add(Paragraph paragraph) {
            this.Text.Add(paragraph);
        }

        public void RemoveAt(int index) {
            this.Text.RemoveAt(index);
        }

        public void Draw(UpdateEventArgs e) {
            // Create backups and set the colour
            Color backupColor = this.FontGeometrie.Color;
            float backupHeight = this.FontGeometrie.Height;
            this.FontGeometrie.Color = this.Colour;
            // Draw the paragraphs with their titles (if any)
            int height = 40;
            for (int i = 0; i < this.Text.Count; i++) {
                if (this.Text[i].Header != "") {
                    this.FontGeometrie.Height = this.TextHeight * 1.5f;
                    this.FontGeometrie.DrawString(new Vector2(this.Position.X, height), this.Text[i].Header);
                    height += (int)(this.TextHeight * 1.5f + 0.5f);
                }
                this.FontGeometrie.Height = this.TextHeight;
                this.FontGeometrie.DrawMultiLineString(new Vector2(this.Position.X + this.ParagraphOffsetX, height), this.Text[i].Text);
                height += this.TextHeight * (this.Text[i].NrOfLines + 1);
            }
            // backup
            this.FontGeometrie.Height = backupHeight;
            this.FontGeometrie.Color = backupColor;
        }
    }

    public class Paragraph
    {
        public string Header { get; private set; }
        public string Text { get; private set; }
        public int NrOfLines { get; private set; }

        public Paragraph()
            : this("") { }
        public Paragraph(string header) {
            this.Header = header;
            this.Text = "";
            this.NrOfLines = 0; // Always one extra line :)
        }

        public void AddLine(string line = "") {
            this.Text += line + System.Environment.NewLine;
            this.NrOfLines++;
        }
    }
}
