using EventManager.Configuration;
using EventManager.Data.Entities.Birthday;
using EventManager.Data.Entities.Restrictions;
using EventManager.Data.Repositories;
using EventManager.Models.Restrictions;
using MediatR;
using NetCord.Gateway;

namespace EventManager.Events.CheckFskRestrictionOnUser;

public class CheckFskRestrictionOnUserEventHandler : IRequestHandler<CheckFskRestrictionOnUserEvent, RestrictionCheckResult>
{
    private readonly RootConfig _config;
    private readonly UserBirthdayRepository _userBirthdayRepository;

    public CheckFskRestrictionOnUserEventHandler(RootConfig config, UserBirthdayRepository userBirthdayRepository)
    {
        _config = config;
        _userBirthdayRepository = userBirthdayRepository;
    }

    public async Task<RestrictionCheckResult> Handle(CheckFskRestrictionOnUserEvent request, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        
        if (request.Event.Restrictions.SingleOrDefault(x => x is FskRestrictions) is not FskRestrictions fskRestriction)
        {
            return new RestrictionCheckResult(true);
        }

        var roles = request.User.RoleIds.ToList();
        FskRange? range = _config.Discord.Fsk.Range.SingleOrDefault(x => roles.Any(z => z == x.RoleId));
        if (range is null)
        {
            return new RestrictionCheckResult(false, "Du hast leider kein Alter angegeben!");
        }

        UserBirthday? userBirthday = await _userBirthdayRepository.GetByDiscordAsync(request.User.Id);

        if (userBirthday is not null && userBirthday.Birthday.Year != 1)
        {
            if (fskRestriction.MaxAlter is not null && fskRestriction.MaxAlter < userBirthday.Birthday.GetAge())
            {
                return new RestrictionCheckResult(false, $"Du bist leider zu Alt für das Event! Das Maximalalter für das Event beträgt {fskRestriction.MaxAlter} Jahre");
            }

            if (fskRestriction.MinAlter is not null && fskRestriction.MinAlter > userBirthday.Birthday.GetAge())
            {
                return new RestrictionCheckResult(false, $"Du bist leider zu Jung für das Event! Das Mindestalter für das Event beträgt {fskRestriction.MinAlter} Jahre");
            }
        }

        if (fskRestriction.MaxAlter is not null)
        {
            if (fskRestriction.MaxAlter <= (range.MaxAge ?? range.MinAge))
            {
                return new RestrictionCheckResult(false, $"Du bist leider zu Alt für das Event! Das Maximalalter für das Event beträgt {fskRestriction.MaxAlter} Jahre");
            }
        }

        if (fskRestriction.MinAlter is not null)
        {
            if (fskRestriction.MinAlter >= (range.MinAge ?? range.MaxAge))
            {
                return new RestrictionCheckResult(false, $"Du bist leider zu Jung für das Event! Das Mindestalter für das Event beträgt {fskRestriction.MinAlter} Jahre");
            }
        }
        
        return new RestrictionCheckResult(true);
    }
}