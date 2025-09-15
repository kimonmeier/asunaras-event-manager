using EventManager.Data.Entities.Events.QOTD;
using EventManager.Data.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace EventManager.Data.Repositories;

public class QotdQuestionRepository : GenericRepository<QotdQuestion>
{
    public QotdQuestionRepository(DbContext dbContext) : base(dbContext)
    {
    }


    public async Task<QotdQuestion?> GetUnusedQuestion()
    {
        IQueryable<QotdQuestion> postedMessages = FindDbSet<QotdMessage>()
            .Include(x => x.Question)
            .GroupBy(x => x.Question)
            .Select(x => x.Key);
        
        var qotdQuestions = await Entities
            .Except(postedMessages)
            .ToListAsync();
        
        return qotdQuestions.FirstOrDefault();
    }
    
    public async Task<QotdQuestion?> GetLeastQuestions()
    {
        await Task.CompletedTask;

        return FindDbSet<QotdMessage>()
            .Include(x => x.Question)
            .ToList()
            .GroupBy(x => x.QuestionId)
            .OrderBy(x => x.Count())
            .FirstOrDefault()?

            .OrderByDescending(x => x.PostedOn)
            .Select(x => x.Question)
            .FirstOrDefault();
    }

    public Task<List<string>> GetQuestionsText()
    {
        return Entities.Select(x => x.Question).ToListAsync();
    }
}