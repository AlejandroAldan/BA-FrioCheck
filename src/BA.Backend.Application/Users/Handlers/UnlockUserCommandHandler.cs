using BA.Backend.Application.Users.Commands;
using BA.Backend.Domain.Repositories;
using MediatR;

namespace BA.Backend.Application.Users.Handlers;

public class UnlockUserCommandHandler : IRequestHandler<UnlockUserCommand, Unit>
{
    private readonly IUserRepository _userRepository;

    public UnlockUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Unit> Handle(UnlockUserCommand request, CancellationToken cancellationToken)
    {
        // 1. Obtener usuario
        var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);
        if (user == null)
            throw new InvalidOperationException("Usuario no existe");

        // 2. Validar tenant match
        if (user.TenantId != request.TenantId)
            throw new InvalidOperationException("No tienes permiso para desbloquear este usuario");

        // 3. Desbloquear
        user.IsLocked = false;

        // 4. Guardar
        await _userRepository.UpdateAsync(user, cancellationToken);

        return Unit.Value;
    }
}
