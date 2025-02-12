﻿namespace NewsProject.Models.DB
{
    public class Tag
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Article? Article { get; set; }
    }
}
