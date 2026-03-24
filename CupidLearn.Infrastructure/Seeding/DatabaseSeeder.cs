using CupidLearn.Domain.Content;
using CupidLearn.Domain.Profiles;
using CupidLearn.Domain.Users;
using CupidLearn.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CupidLearn.Infrastructure.Seeding;

public class DatabaseSeeder(
    AppDbContext db,
    IOptions<AdminSeedOptions> adminSeedOptionsAccessor)
{
    private readonly AdminSeedOptions _adminSeedOptions = adminSeedOptionsAccessor.Value;
    private readonly PasswordHasher<AppUser> _passwordHasher = new();

    public async Task SeedAsync(CancellationToken ct)
    {
        await SeedAdminAsync(ct);
        await SeedActivityTypesAsync(ct);
        await SeedAlphabetModuleAsync(ct);
        await SeedNumbersModuleAsync(ct);
    }

    private async Task SeedAdminAsync(CancellationToken ct)
    {
        if (!_adminSeedOptions.Enabled)
        {
            return;
        }

        var anyAdmin = await db.Users.AnyAsync(x => x.Role == "ADMIN", ct);
        if (anyAdmin)
        {
            return;
        }

        var email = _adminSeedOptions.Email.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(_adminSeedOptions.Password))
        {
            return;
        }

        var existingUser = await db.Users.FirstOrDefaultAsync(x => x.Email == email, ct);
        if (existingUser != null)
        {
            existingUser.Role = "ADMIN";
            existingUser.IsVerified = true;
            existingUser.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(ct);
            return;
        }

        var admin = new AppUser
        {
            Email = email,
            Role = "ADMIN",
            IsVerified = true
        };

        admin.PasswordHash = _passwordHasher.HashPassword(admin, _adminSeedOptions.Password);

        db.Users.Add(admin);
        db.UserProfiles.Add(new UserProfile { UserId = admin.Id });

        await db.SaveChangesAsync(ct);
    }

    private async Task SeedAlphabetModuleAsync(CancellationToken ct)
    {
        var levelId = Guid.Parse("a2ee7703-8cdd-42e4-8653-506d8ee96b32");
        var moduleId = Guid.Parse("54f26e19-7577-4310-9c27-e5f9b571a49f");

        var level = await db.Levels.FirstOrDefaultAsync(x => x.Id == levelId, ct);
        if (level == null)
        {
            level = new CupidLearn.Domain.Content.Level
            {
                Id = levelId,
                Language = "en",
                Name = "Beginner"
            };
            db.Levels.Add(level);
        }

        var module = await db.Modules.FirstOrDefaultAsync(x => x.Id == moduleId, ct);
        if (module == null)
        {
            module = new CupidLearn.Domain.Content.Module
            {
                Id = moduleId,
                LevelId = levelId,
                Name = "Alphabet",
                OrderIndex = 0
            };
            db.Modules.Add(module);
        }

        await db.SaveChangesAsync(ct);

        var anyLesson = await db.Lessons.AnyAsync(x => x.ModuleId == moduleId, ct);
        if (anyLesson)
        {
            return;
        }

        var alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        for (int i = 0; i < alphabet.Length; i++)
        {
            var letter = alphabet[i].ToString();
            var letterLower = letter.ToLowerInvariant();

            var lesson = new CupidLearn.Domain.Content.Lesson
            {
                Id = Guid.NewGuid(),
                ModuleId = moduleId,
                Title = $"Letter {letter}",
                Description = $"Learn the letter {letter}",
                OrderIndex = i
            };

            db.Lessons.Add(lesson);

            db.LessonActivities.Add(new CupidLearn.Domain.Content.LessonActivity
            {
                Id = Guid.NewGuid(),
                LessonId = lesson.Id,
                Type = "FLASHCARD",
                Title = $"Learn {letter}",
                OrderIndex = 0,
                PayloadJson = $$"""
                {
                    "version": 1,
                    "cardTitle": "{{letter}}",
                    "cardText": "{{letter}} is for ...",
                    "cardImageUrl": "https://anglomaniacy.pl/img/{{letterLower}}-fun.png",
                    "cardAudioUrl": ""
                }
                """
            });

            db.LessonActivities.Add(new CupidLearn.Domain.Content.LessonActivity
            {
                Id = Guid.NewGuid(),
                LessonId = lesson.Id,
                Type = "MCQ",
                Title = $"Find {letter}",
                OrderIndex = 1,
                PayloadJson = $$"""
                {
                    "version": 1,
                    "questionText": "Find the letter {{letter}}",
                    "options": [
                        "{{letter}}",
                        "X",
                        "Y",
                        "Z"
                    ],
                    "correctIndex": 0
                }
                """
            });

            db.LessonActivities.Add(new CupidLearn.Domain.Content.LessonActivity
            {
                Id = Guid.NewGuid(),
                LessonId = lesson.Id,
                Type = "MEMORY",
                Title = $"Match {letter}",
                OrderIndex = 2,
                PayloadJson = $$"""
                {
                    "version": 2,
                    "items": [
                        "{{letter}}",
                        "https://anglomaniacy.pl/img/{{letterLower}}-fun.png"
                    ]
                }
                """
            });

            db.LessonActivities.Add(new CupidLearn.Domain.Content.LessonActivity
            {
                Id = Guid.NewGuid(),
                LessonId = lesson.Id,
                Type = "SHADOW_MATCH",
                Title = $"Shadow {letter}",
                OrderIndex = 3,
                PayloadJson = $$"""
                {
                    "version": 1,
                    "mainImageUrl": "https://anglomaniacy.pl/img/{{letterLower}}-fun.png",
                    "options": [
                        "https://anglomaniacy.pl/img/b-fun.png",
                        "https://anglomaniacy.pl/img/{{letterLower}}-fun.png",
                        "https://anglomaniacy.pl/img/c-fun.png"
                    ],
                    "correctIndex": 1
                }
                """
            });
        }

        await db.SaveChangesAsync(ct);
    }

    private async Task SeedNumbersModuleAsync(CancellationToken ct)
    {
        var levelId = Guid.Parse("a2ee7703-8cdd-42e4-8653-506d8ee96b32");
        var moduleId = Guid.Parse("d1b7e2a9-4b4d-4e9a-9f1a-2b3c4d5e6f7b");

        var level = await db.Levels.FirstOrDefaultAsync(x => x.Id == levelId, ct);
        if (level == null)
        {
            level = new CupidLearn.Domain.Content.Level
            {
                Id = levelId,
                Language = "en",
                Name = "Beginner"
            };
            db.Levels.Add(level);
        }

        var module = await db.Modules.FirstOrDefaultAsync(x => x.Id == moduleId, ct);
        if (module == null)
        {
            module = new CupidLearn.Domain.Content.Module
            {
                Id = moduleId,
                LevelId = levelId,
                Name = "Numbers",
                OrderIndex = 1
            };
            db.Modules.Add(module);
        }

        await db.SaveChangesAsync(ct);

        var anyLesson = await db.Lessons.AnyAsync(x => x.ModuleId == moduleId, ct);
        if (anyLesson)
        {
            return;
        }

        var numbers = new[] { "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten" };
        var activityPool = new[] { "MULTIPLE_CHOICE", "SHADOW_MATCH", "MEMORY_MATCH", "WORD_BUILDER", "SORTING_BINS", "STORY_SEQUENCER" };

        for (int i = 0; i < numbers.Length; i++)
        {
            var numberName = numbers[i];
            var val = i + 1;
            var lessonId = Guid.NewGuid();

            var lesson = new CupidLearn.Domain.Content.Lesson
            {
                Id = lessonId,
                ModuleId = moduleId,
                Title = $"Number {val}",
                Description = $"Learn the number {val} ({numberName})",
                OrderIndex = i
            };
            db.Lessons.Add(lesson);

            // 1. Flashcard (Fixed)
            db.LessonActivities.Add(new CupidLearn.Domain.Content.LessonActivity
            {
                Id = Guid.NewGuid(),
                LessonId = lessonId,
                Type = "FLASHCARD",
                Title = $"Learn {val}",
                OrderIndex = 0,
                PayloadJson = $$"""
                {
                    "version": 1,
                    "cardTitle": "{{val}}",
                    "cardText": "{{val}} is {{numberName}}",
                    "cardImageUrl": "https://anglomaniacy.pl/img/a-{{numberName}}.png",
                    "cardAudioUrl": ""
                }
                """
            });

            // Randomly select 3 other activity types
            var randomTypes = activityPool.OrderBy(x => Guid.NewGuid()).Take(3).ToList();

            for (int j = 0; j < randomTypes.Count; j++)
            {
                var type = randomTypes[j];
                var activityId = Guid.NewGuid();
                string payload = "";

                if (type == "MULTIPLE_CHOICE")
                {
                    var options = new List<string> { val.ToString(), (val + 1).ToString(), (val - 1).ToString(), (val + 2).ToString() };
                    options = options.OrderBy(x => Guid.NewGuid()).ToList();
                    var correctIndex = options.IndexOf(val.ToString());
                    payload = $$"""
                    {
                        "version": 1,
                        "questionText": "Find the number {{val}}",
                        "options": [{{string.Join(",", options.Select(o => $"\"{o}\""))}}],
                        "correctIndex": {{correctIndex}}
                    }
                    """;
                }
                else if (type == "SHADOW_MATCH")
                {
                    var options = new List<string> { 
                        $"https://anglomaniacy.pl/img/a-{numberName}.png",
                        $"https://anglomaniacy.pl/img/a-{numbers[(i + 1) % 10]}.png",
                        $"https://anglomaniacy.pl/img/a-{numbers[(i + 2) % 10]}.png"
                    };
                    options = options.OrderBy(x => Guid.NewGuid()).ToList();
                    var correctIndex = options.IndexOf($"https://anglomaniacy.pl/img/a-{numberName}.png");
                    payload = $$"""
                    {
                        "version": 1,
                        "mainImageUrl": "https://anglomaniacy.pl/img/a-{{numberName}}.png",
                        "options": [{{string.Join(",", options.Select(o => $"\"{o}\""))}}],
                        "correctIndex": {{correctIndex}}
                    }
                    """;
                }
                else if (type == "MEMORY_MATCH")
                {
                    payload = $$"""
                    {
                        "version": 2,
                        "items": [
                            "{{val}}",
                            "https://anglomaniacy.pl/img/a-{{numberName}}.png"
                        ]
                    }
                    """;
                }
                else if (type == "WORD_BUILDER")
                {
                    payload = $$"""
                    {
                        "version": 1,
                        "targetImageUrl": "https://anglomaniacy.pl/img/a-{{numberName}}.png",
                        "wordString": "{{numberName}}",
                        "case": "lower"
                    }
                    """;
                }
                else if (type == "SORTING_BINS")
                {
                    payload = $$"""
                    {
                        "version": 1,
                        "categories": ["Numbers", "Others"],
                        "items": [
                            {"text": "{{val}}", "categoryIndex": 0},
                            {"text": "{{(val + 1) % 11}}", "categoryIndex": 1}
                        ]
                    }
                    """;
                }
                else if (type == "STORY_SEQUENCER")
                {
                    var seq = new List<string> {
                        $"https://anglomaniacy.pl/img/a-{numbers[i % 10]}.png",
                        $"https://anglomaniacy.pl/img/a-{(i + 1 < 10 ? numbers[i + 1] : numbers[0])}.png",
                        $"https://anglomaniacy.pl/img/a-{(i + 2 < 10 ? numbers[i + 2] : numbers[1])}.png"
                    };
                    payload = $$"""
                    {
                        "version": 1,
                        "images": [{{string.Join(",", seq.Select(s => $"\"{s}\""))}}]
                    }
                    """;
                }

                db.LessonActivities.Add(new CupidLearn.Domain.Content.LessonActivity
                {
                    Id = activityId,
                    LessonId = lessonId,
                    Type = type,
                    Title = $"{type.Replace("_", " ")} Practice",
                    OrderIndex = j + 1,
                    PayloadJson = payload
                });
            }
        }

        await db.SaveChangesAsync(ct);
    }

    private async Task SeedActivityTypesAsync(CancellationToken ct)
    {
        var types = new List<ActivityType>
        {
            new() { 
                Key = "MULTIPLE_CHOICE", 
                DisplayName = "Multiple Choice (Bubble Pop / Tap)", 
                SchemaJson = """
                {
                  "$schema": "https://json-schema.org/draft/2020-12/schema",
                  "title": "Multiple Choice (Bubble Pop / Tap)",
                  "type": "object",
                  "properties": {
                    "version": { "type": "integer", "const": 1 },
                    "questionAudioUrl": { "type": "string", "format": "uri" },
                    "questionText": { "type": "string" },
                    "options": { "type": "array", "minItems": 2, "items": { "type": "string" } },
                    "correctIndex": { "type": "integer", "minimum": 0 }
                  },
                  "required": ["version", "options", "correctIndex"],
                  "ui": {
                    "order": ["questionAudioUrl", "questionText", "options", "correctIndex"],
                    "widgets": {
                      "questionAudioUrl": { "type": "file-url" },
                      "options": { "type": "string-array" }
                    }
                  }
                }
                """
            },
            new() { 
                Key = "MAGIC_TRACE", 
                DisplayName = "Magic Trace (Writing Practice)", 
                SchemaJson = """
                {
                  "$schema": "https://json-schema.org/draft/2020-12/schema",
                  "title": "Magic Trace (Writing Practice)",
                  "type": "object",
                  "properties": {
                    "version": { "type": "integer", "const": 3 },
                    "instructionAudioUrl": { "type": "string", "format": "uri" },
                    "imageUrl": { "type": "string", "format": "uri" },
                    "tolerancePercent": { "type": "integer", "default": 80 }
                  },
                  "required": ["version", "imageUrl", "instructionAudioUrl"],
                  "ui": {
                    "widgets": {
                      "instructionAudioUrl": { "type": "file-url" },
                      "imageUrl": { "type": "file-url" }
                    }
                  }
                }
                """
            },
            new() { 
                Key = "SHADOW_MATCH", 
                DisplayName = "Shadow Match", 
                SchemaJson = """
                {
                  "$schema": "https://json-schema.org/draft/2020-12/schema",
                  "title": "Shadow Match",
                  "type": "object",
                  "properties": {
                    "version": { "type": "integer", "const": 1 },
                    "mainImageUrl": { "type": "string", "format": "uri" },
                    "options": { "type": "array", "items": { "type": "string", "format": "uri" } },
                    "correctIndex": { "type": "integer" }
                  },
                  "required": ["version", "mainImageUrl", "options", "correctIndex"],
                  "ui": {
                    "widgets": {
                      "mainImageUrl": { "type": "file-url" },
                      "options": { "type": "file-url-array" }
                    }
                  }
                }
                """
            },
            new() { 
                Key = "WORD_BUILDER", 
                DisplayName = "Word Builder (Letter Train)", 
                SchemaJson = """
                {
                   "type": "object",
                   "properties": {
                     "version": { "type": "integer", "const": 1 },
                     "targetImageUrl": { "type": "string", "format": "uri" },
                     "wordString": { "type": "string" },
                     "case": { "type": "string", "enum": ["upper", "lower"] }
                   },
                   "required": ["version", "targetImageUrl", "wordString"],
                   "ui": { "widgets": { "targetImageUrl": { "type": "file-url" } } }
                }
                """
            },
            new() { 
                Key = "SORTING_BINS", 
                DisplayName = "Sorting Bins", 
                SchemaJson = """
                {
                  "type": "object",
                  "properties": {
                    "version": { "type": "integer", "const": 1 },
                    "categories": { "type": "array", "items": { "type": "string" } },
                    "items": { "type": "array", "items": { "type": "object" } }
                  },
                  "required": ["version", "categories", "items"]
                }
                """
            },
            new() { 
                Key = "PARROT_MIC", 
                DisplayName = "Parrot Mic", 
                SchemaJson = """
                {
                  "type": "object",
                  "properties": {
                    "version": { "type": "integer", "const": 1 },
                    "modelAudioUrl": { "type": "string", "format": "uri" },
                    "targetText": { "type": "string" }
                  },
                  "required": ["version", "modelAudioUrl", "targetText"],
                  "ui": { "widgets": { "modelAudioUrl": { "type": "file-url" } } }
                }
                """
            },
            new() { 
                Key = "STORY_SEQUENCER", 
                DisplayName = "Story Sequencer", 
                SchemaJson = """
                {
                  "type": "object",
                  "properties": {
                    "version": { "type": "integer", "const": 1 },
                    "images": { "type": "array", "items": { "type": "string", "format": "uri" } }
                  },
                  "required": ["version", "images"],
                  "ui": { "widgets": { "images": { "type": "file-url-array" } } }
                }
                """
            },
            new() { 
                Key = "MEMORY_MATCH", 
                DisplayName = "Memory Match", 
                SchemaJson = """
                {
                  "type": "object",
                  "properties": {
                    "version": { "type": "integer", "const": 2 },
                    "items": { "type": "array", "items": { "type": "string" } }
                  },
                  "required": ["version", "items"],
                  "ui": { "widgets": { "items": { "type": "string-array" } } }
                }
                """
            },
            new() { 
                Key = "FLASHCARD", 
                DisplayName = "Flashcard", 
                SchemaJson = """
                {
                  "type": "object",
                  "properties": {
                    "version": { "type": "integer", "const": 1 },
                    "cardTitle": { "type": "string" },
                    "cardText": { "type": "string" },
                    "cardImageUrl": { "type": "string", "format": "uri" },
                    "cardAudioUrl": { "type": "string", "format": "uri" }
                  },
                  "required": ["version", "cardTitle"],
                  "ui": {
                    "widgets": {
                      "cardImageUrl": { "type": "file-url" },
                      "cardAudioUrl": { "type": "file-url" }
                    }
                  }
                }
                """
            }
        };

        foreach (var type in types)
        {
            var existing = await db.ActivityTypes.FirstOrDefaultAsync(x => x.Key == type.Key, ct);
            if (existing == null)
            {
                db.ActivityTypes.Add(type);
            }
            else
            {
                existing.DisplayName = type.DisplayName;
                existing.SchemaJson = type.SchemaJson;
                existing.UpdatedAt = DateTimeOffset.UtcNow;
            }
        }

        await db.SaveChangesAsync(ct);
    }
}
