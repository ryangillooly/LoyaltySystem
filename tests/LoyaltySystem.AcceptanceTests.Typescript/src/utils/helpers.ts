import { RegisterUserDto } from '../models/auth.models'

export function createRegisterRequest(
    user: string,
    roles: string[] = ['SuperAdmin'])
{
    const unique = Math.random().toString(36).slice(2)
    return new RegisterUserDto(
        user,
        user,
        `${user}_${unique}`,
        `${user}_${unique}@example.com`,
        `+447${Math.floor(100000000 + Math.random() * 900000000)}`,
        user,
        user,
        roles
    )
}