import { CrudPanel } from '../../components'
import { PermissionManager } from './PermissionManager'

export function AdminSection({ data, openCreate, openEdit, deleteEntity, getPanelProps }) {
  return (
    <>
      <PermissionManager users={data.users} />
      <CrudPanel entityKey="users" rows={data.users} onCreate={openCreate} onEdit={openEdit} onDelete={deleteEntity} panelProps={getPanelProps('admin-users')} />
      <CrudPanel entityKey="roles" rows={data.roles} onCreate={openCreate} onEdit={openEdit} onDelete={deleteEntity} panelProps={getPanelProps('admin-roles')} />
    </>
  )
}
